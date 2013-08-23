using NUnit.Framework;
using SS.Architecture.Cache.Redis.Configuration;
using SS.Architecture.Cache.Redis.Sentinel;
using System;
using System.Linq;

namespace SS.Architecture.Cache.Redis.Tests.SentinelClient
{
    public class RedisSentinelClientFacts
    {
        protected RedisClientConfig MasterConfig;
        protected RedisClientConfig SentinelConfig;

        public RedisSentinelClientFacts()
        {
            MasterConfig = new RedisClientConfig()
            {
                AddressName = "testmaster"
            };

            SentinelConfig = new RedisClientConfig
            {
                IpAddress = "10.11.51.30",
                Port = "26379"
            };

        }

        public class TheGetMasterByAddressNameMethod : RedisSentinelClientFacts
        {
            [Test]
            public void ShouldReturnTheIpAndPortOfMasterIfActive()
            {
                var sentinel = new RedisSentinelClient(SentinelConfig.IpAddress, Convert.ToInt32(SentinelConfig.Port));

                var result = sentinel.GetMasterByAddressName(MasterConfig.AddressName);
                Assert.That(result != null);
                Assert.That(!string.IsNullOrWhiteSpace(result.Item1));
                Assert.That(0 < result.Item2);
            }
        }

        public class TheGetSlavesByAddressNameMethod : RedisSentinelClientFacts
        {
            [Test]
            public void ShouldListAvailableSlaves()
            {
                var sentinel = new RedisSentinelClient(SentinelConfig.IpAddress, Convert.ToInt32(SentinelConfig.Port));
                var result = sentinel.GetSlavesByAddressName(MasterConfig.AddressName);
                Assert.NotNull(result);
                Assert.That(result.Any());
            }
        }

        public class TheIsMasterDownMethod : RedisSentinelClientFacts
        {
            [Test, Ignore]
            //Sentinel Feature not working
            public void ShouldReturnFalseIfCanContactMaster()
            {
                var sentinel = new RedisSentinelClient(SentinelConfig.IpAddress, Convert.ToInt32(SentinelConfig.Port));
                var result = sentinel.IsMasterDown(MasterConfig.AddressName);
                Assert.False(result);
            }
        }
    }
}
