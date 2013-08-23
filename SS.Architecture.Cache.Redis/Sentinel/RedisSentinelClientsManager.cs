using SS.Architecture.Cache.Redis.Exceptions;
using SS.Architecture.Interfaces.Caching;
using SS.Architecture.Interfaces.Caching.Monitoring;
using SS.Architecture.Logging.Contract;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SS.Architecture.Cache.Redis.Sentinel
{
    public class RedisSentinelClientsManager : ISentinelClientsManager
    {
        private const int DefaultSentinelsPingTimeout = 10;
        protected readonly ILogger Logger;
        protected readonly ICacheConfig Config;

        private readonly int _sentinelsPingTimeout;
        private readonly IEnumerable<ICacheClientConfig> _availableSentinels;

        private ISentinelClient _lastAvailableSentinel;

        public RedisSentinelClientsManager(ILogger logger, ICacheConfig config)
        {
            Contract.Assert(logger != null);
            Contract.Assert(config != null);

            Logger = logger;
            Config = config;

            _sentinelsPingTimeout = config.SentinelPingTimeout > 0
                                        ? config.SentinelPingTimeout
                                        : DefaultSentinelsPingTimeout;

            _availableSentinels = config.SentinelClients;
        }

        public ISentinelClient GetActiveSentinel()
        {
            //if a sentinel already reply try to contact it (ping) with timeout of 10ms
            if (_lastAvailableSentinel != null)
            {
                if (IsSentinelAvailable(_lastAvailableSentinel))
                {
                    return _lastAvailableSentinel;
                }
            }

            foreach (ICacheClientConfig sentinelCfg in _availableSentinels)
            {
                var sentinelClient = new RedisSentinelClient(sentinelCfg.IpAddress, Convert.ToInt32(sentinelCfg.Port));

                if (IsSentinelAvailable(sentinelClient))
                {
                    _lastAvailableSentinel = sentinelClient;
                    return sentinelClient;
                }
            }

            throw new NoSentinelAvailableException();
        }

        public void RefreshSentinels()
        {
            throw new NotImplementedException();
        }

        private bool IsSentinelAvailable(ISentinelClient sentinelToPing)
        {
            Contract.Assert(sentinelToPing != null);

            var result = false;
            var resetEvent = new ManualResetEvent(false);

            Action pingSentinel = () =>
                {
                    result = sentinelToPing.Ping();
                    resetEvent.Set();        
                };

            Task.Run(pingSentinel);
            resetEvent.WaitOne(_sentinelsPingTimeout);
            return result;
        }
    }
}
