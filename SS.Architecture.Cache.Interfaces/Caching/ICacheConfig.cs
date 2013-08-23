
using System.Collections.Generic;

namespace SS.Architecture.Interfaces.Caching
{
    public interface ICacheConfig
    {
        string RedisCacheKeyPrefix { get; set; }
        bool UseCache { get; set; }

        int SentinelPingTimeout { get; set; }

        IEnumerable<ICacheClientConfig> CacheClients { get;  }
        IEnumerable<ICacheClientConfig> SentinelClients { get; } 
    }
}
