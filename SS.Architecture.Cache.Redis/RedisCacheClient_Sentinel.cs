using System.Configuration;
using SS.Architecture.Cache.Redis.Exceptions;
using ServiceStack.Redis;
using SS.Architecture.Interfaces.Caching;
using SS.Architecture.Cache.Redis.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Architecture.Cache.Redis
{
    public partial class RedisCacheClient
    {
        protected bool IsSentinelModeAvailable { get { return SentinelClientsManager != null && _sentinelConfigValid; } }
        private bool _sentinelConfigValid;
        protected bool IsRedisModelAvailable { get { return _clientsManager != null; } }
        private IRedisClientsManager _clientsManager;

        private string _masterAddressName;

        private void Initialize()
        {
            //If Clients defined and if these clients have a valid config 
            //then create a redis Pooled client manager.
            if (Config.CacheClients.Any())
            {
                var masters = Config.CacheClients.Where(x => x.IsRedisHostConfigValid()).Select(x => x.FullAddress()).ToArray();

                if (masters.Any())
                {
                    _clientsManager = new PooledRedisClientManager(masters);
                }
            }

            //ensure that the minimum amount of info is defined to run sentinel mode.
            _sentinelConfigValid = Config.SentinelClients.Any(x => x.IsRedisHostConfigValid()) &&
                                   Config.CacheClients.Any(x => x.HasAddressName());

            if (_sentinelConfigValid)
            {
                _masterAddressName = Config.CacheClients.First(x => x.HasAddressName()).AddressName;
            }
        }

        protected IRedisClient GetRedisClient()
        {
            if (IsSentinelModeAvailable)
            {
                var master = SentinelClientsManager.GetActiveSentinel().GetMasterByAddressName(_masterAddressName);
                if (master == null) { throw new RedisCommunicationException(string.Format("No master found for address {0}.", _masterAddressName)); }
                return new RedisClient(master.Item1, master.Item2);
            }

            if (IsRedisModelAvailable)
            {
                return _clientsManager.GetClient();
            }

            //For Redis Mode. Need at least one node with ip+port
            //For sentinel mode Need at least one node with address name + at least one sentinel node with ip+port
            throw new ConfigurationErrorsException("Invalid Redis Configuration Section. Please ensure that all necessary information are defined in the config.");
        }

        protected IRedisClient GetRedisReadOnlyClient()
        {
            if (IsSentinelModeAvailable)
            {
                var slaves = SentinelClientsManager.GetActiveSentinel().GetSlavesByAddressName(_masterAddressName).ToArray();
                if (!slaves.Any())
                {
                    return GetRedisClient();
                }
                var selectedSlave = slaves.First();
                return new RedisClient(selectedSlave.Item1, selectedSlave.Item2);
            }

            if (IsRedisModelAvailable)
            {
                return _clientsManager.GetReadOnlyClient();
            }

            //For Redis Mode. Need at least one node with ip+port
            //For sentinel mode Need at least one node with address name + at least one sentinel node with ip+port
            throw new ConfigurationErrorsException("Invalid Redis Configuration Section. Please ensure that all necessary information are defined in the config.");
        }

    }
}
