using System.Configuration;
using System.Xml;
using System.Xml.Serialization;

namespace SS.Architecture.Cache.Redis.Configuration
{
    [XmlRoot("redisCacheConfigSection")]
    public class RedisCacheConfigSectionHandler : IConfigurationSectionHandler
    {
        public object Create(object parent, object configContext, XmlNode section)
        {
            var reader = new XmlNodeReader(section);
            var xmlSerializer = new XmlSerializer(typeof(RedisCacheConfigSectionHandler));
            var redisConfiguration = xmlSerializer.Deserialize(reader);
            return redisConfiguration;
        }

        [XmlElement("redisCache")]
        public RedisCacheConfig RedisCacheConfig { get; set; }
    }
}
