using SS.Architecture.Interfaces.Caching;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace SS.Architecture.Cache.Redis.Configuration
{
    [XmlRoot("redisCache")]
    public sealed class RedisCacheConfig : ICacheConfig
    {
        //Needed for serialization
        private RedisCacheConfig()
            : this(new List<RedisClientConfig>(), new List<RedisClientConfig>())
        {

        }

        public RedisCacheConfig(IEnumerable<RedisClientConfig> cacheClients, IEnumerable<RedisClientConfig> sentinelClients)
        {
            RedisClients = cacheClients.ToList();
            RedisSentinelsClients = sentinelClients.ToList();
        }

        [XmlElement("cachePrefix")]
        public string RedisCacheKeyPrefix { get; set; }

        [XmlElement("MaxReadPoolSize")]
        public int MaxReadPoolSize { get; set; }
        
        [XmlElement("MaxWritePoolSize")]
        public int MaxWritePoolSize { get; set; }

        [XmlElement("PoolTimeOut")]
        public int PoolTimeOut { get; set; }

        [XmlAttribute]
        public bool UseCache { get; set; }

        [XmlArray("redisClients")]
        [XmlArrayItem("redis", typeof(RedisClientConfig))]
        public List<RedisClientConfig> RedisClients { get; set; }

        [XmlArray("sentinelClients")]
        [XmlArrayItem("sentinel", typeof(RedisClientConfig))]
        public List<RedisClientConfig> RedisSentinelsClients { get; set; }

        [XmlIgnore]
        public IEnumerable<ICacheClientConfig> CacheClients
        {
            get { return RedisClients; }
        }

        [XmlIgnore]
        public IEnumerable<ICacheClientConfig> SentinelClients
        {
            get { return RedisSentinelsClients; }

        }

        [XmlElement("sentinelsPingTimeout", typeof(System.Int32))]
        public int SentinelPingTimeout { get; set; }
    }
}
