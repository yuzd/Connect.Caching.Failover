using SS.Architecture.Interfaces.Caching;
using System.Xml.Serialization;

namespace SS.Architecture.Cache.Redis.Configuration
{
    public sealed class RedisClientConfig : ICacheClientConfig
    {
        [XmlAttribute]
        public string IpAddress { get; set; }

        [XmlAttribute]
        public string Port { get; set; }

        [XmlAttribute]
        public string Password { get; set; }

        [XmlAttribute]
        public string AddressName { get; set; }

    }
}
