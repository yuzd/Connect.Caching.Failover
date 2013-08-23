using NUnit.Framework;
using System.Configuration;
using System.Linq;
using SS.Architecture.Cache.Redis.Configuration;

namespace SS.Architecture.Cache.Redis.Tests
{
    public class RedisConfigurationSectionFacts
    {
        [Test]
        public void CanLoadFromConfigFile()
        {
            var configuration = ((RedisCacheConfigSectionHandler)ConfigurationManager.GetSection("redisCacheConfigSection")).RedisCacheConfig;

            Assert.AreEqual(true, configuration.UseCache);
            Assert.AreEqual("Test", configuration.RedisCacheKeyPrefix);
            Assert.AreEqual(10, configuration.SentinelPingTimeout);
            Assert.AreEqual(1, configuration.RedisClients.Count());
            Assert.AreEqual("cuigtpdatreu001", configuration.RedisClients.First().AddressName);
            Assert.AreEqual(3, configuration.SentinelClients.Count());

            var firstSentinel = configuration.SentinelClients.First();
            Assert.AreEqual("95.138.180.8", firstSentinel.IpAddress);
            Assert.AreEqual("26379", firstSentinel.Port);
        }
    }
}
