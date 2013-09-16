using SS.Architecture.Cache.Redis.Exceptions;
using SS.Architecture.Interfaces.Caching;
using SS.Architecture.Interfaces.Caching.Monitoring;
using SS.Architecture.Logging.Contract;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SS.Architecture.Cache.Redis.Sentinel
{
    public partial class RedisSentinelClientsManager : ISentinelClientsManager
    {
        protected const int DefaultSentinelsPingTimeout = 10;
        protected readonly ILogger Logger;
        protected readonly ICacheConfig Config;

        private readonly int _sentinelsPingTimeout;

        private readonly RedisSentinelClient[] _sentinelClients = new RedisSentinelClient[0];
        private RedisSentinelClient _lastAvailableSentinel;

        public RedisSentinelClientsManager(ILogger logger, ICacheConfig config)
        {
            Contract.Assert(logger != null);
            Contract.Assert(config != null);

            Logger = logger;
            Config = config;
            PoolTimeout = config.PoolTimeOut;

            _sentinelsPingTimeout = config.SentinelPingTimeout > 0
                                        ? config.SentinelPingTimeout
                                        : DefaultSentinelsPingTimeout;

            _sentinelClients = config.SentinelClients.Select(x =>
                new RedisSentinelClient(
                    x.IpAddress,
                    Convert.ToInt32(x.Port),
                    string.IsNullOrWhiteSpace(x.Password)
                        ? null
                        : x.Password) { ClientsManager = this }
                ).ToArray();

            OnStart();
        }

        public ISentinelClient GetActiveSentinel()
        {
            //if a sentinel already reply try to contact it (ping) with timeout of 10ms
            if (_lastAvailableSentinel != null && !_lastAvailableSentinel.Active && !_lastAvailableSentinel.HadExceptions)
            {
                if (IsSentinelAvailable(_lastAvailableSentinel))
                {
                    return _lastAvailableSentinel;
                }
            }

            lock (_sentinelClients)
            {
                RedisSentinelClient inActiveClient = null;

                while ((inActiveClient = GetInActiveSentinel()) == null)
                {
                    if (PoolTimeout > 0)
                    {
                        // wait for a connection, cry out if made to wait too long
                        if (!Monitor.Wait(_sentinelClients, PoolTimeout))
                            throw new NoSentinelAvailableException();
                    }
                    else
                        Monitor.Wait(_sentinelClients, RecheckPoolAfterMs);
                }
                inActiveClient.Active = true;
                return inActiveClient;
            }
        }

        public RedisSentinelClient GetInActiveSentinel()
        {
            for (var i = 0; i < _sentinelClients.Length; i++)
            {
                if (_sentinelClients[i] != null && !_sentinelClients[i].Active)
                {
                    if (!_sentinelClients[i].HadExceptions) return _sentinelClients[i];

                    var sentinelInfo = new Tuple<string, int>(_sentinelClients[i].Host, _sentinelClients[i].Port);
                    _sentinelClients[i].Dispose();
                    _sentinelClients[i] = new RedisSentinelClient(sentinelInfo.Item1, sentinelInfo.Item2) {ClientsManager = this};
                    return _sentinelClients[i];
                }
            }
            return null;
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

        internal void DisposeSentinelClient(RedisSentinelClient sentinelClient)
        {
            lock (_sentinelClients)
            {
                for (var i = 0; i < _sentinelClients.Length; i++)
                {
                    var client = _sentinelClients[i];
                    if (client != sentinelClient) continue;
                    client.Active = false;
                    Monitor.PulseAll(_sentinelClients);
                    return;
                }

                Monitor.PulseAll(_sentinelClients);
            }
        }

    }
}
