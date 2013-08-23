using NUnit.Framework;
using SS.Architecture.Cache.Redis.Configuration;
using SS.Architecture.Cache.Redis.Extensions;
using System;

namespace SS.Architecture.Cache.Redis.Tests.Extensions
{
    public class CacheClientsExtensionsFacts
    {
        public class TheIsRedisHostConfigValidMethod
        {
            [Test]
            public void ShouldReturnFalseIfClientIsNull()
            {
                var cacheConfig = default(RedisClientConfig);
                Assert.False(cacheConfig.IsRedisHostConfigValid());
            }

            [Test]
            public void ShouldReturnFalseIfIpNotValid()
            {
                var config = new RedisClientConfig
                    {
                        AddressName = "",
                        IpAddress = "999.999.999.999",
                        Password = "",
                        Port = "17"
                    };

                Assert.False(config.IsRedisHostConfigValid());
            }

            [Test]
            public void ShouldReturnFalseIfPortNotValid()
            {
                var config = new RedisClientConfig
                {
                    AddressName = "",
                    IpAddress = "127.0.0.1",
                    Password = "",
                    Port = "99999999999"
                };

                Assert.False(config.IsRedisHostConfigValid());
            }

            [Test]
            public void ShouldReturnTrueIfIpAndPortValid()
            {
                var config = new RedisClientConfig
                {
                    AddressName = "",
                    IpAddress = "127.0.0.1",
                    Password = "",
                    Port = "999"
                };
                Assert.True(config.IsRedisHostConfigValid());
            }
        }

        public class TheHasAddressNameMethod
        {
            [Test]
            public void ShouldReturnFalseIfClientNull()
            {
                var cacheConfig = default(RedisClientConfig);
                Assert.False(cacheConfig.HasAddressName());
            }

            [Test]
            public void ShouldReturnFalseIfAddressNameEmpty()
            {
                var config = new RedisClientConfig
                {
                    AddressName = "",
                    IpAddress = "999.999.999.999",
                    Password = "",
                    Port = "17"
                };

                Assert.False(config.HasAddressName());
            }

            [Test]
            public void ShouldReturnFalseIfAddressNameWhiteSpace()
            {
                var config = new RedisClientConfig
                {
                    AddressName = "             ",
                    IpAddress = "999.999.999.999",
                    Password = "",
                    Port = "17"
                };

                Assert.False(config.HasAddressName());
            }

            [Test]
            public void ShouldReturnTrueIfAddressNameNotEmpty()
            {
                var config = new RedisClientConfig
                {
                    AddressName = "Test",
                    IpAddress = "999.999.999.999",
                    Password = "",
                    Port = "17"
                };

                Assert.True(config.HasAddressName());
            }
        }

        public class TheRequirePasswordMethod
        {
            [Test]
            public void ShouldReturnFalseIfClientNull()
            {
                var cacheConfig = default(RedisClientConfig);
                Assert.False(cacheConfig.RequirePassword());
            }

            [Test]
            public void ShouldReturnFalseIfPasswordEmpty()
            {
                var config = new RedisClientConfig
                {
                    AddressName = "             ",
                    IpAddress = "999.999.999.999",
                    Password = "",
                    Port = "17"
                };

                Assert.False(config.RequirePassword());
            }

            [Test]
            public void ShouldReturnTrueIfPasswordSet()
            {
                var config = new RedisClientConfig
                {
                    AddressName = "             ",
                    IpAddress = "999.999.999.999",
                    Password = "Test",
                    Port = "17"
                };

                Assert.True(config.RequirePassword());
            }
        }

        public class TheFullAddressMethod
        {
            [Test]
            public void ShouldThrowOperationExceptionIfClientNull()
            {
                var cacheConfig = default(RedisClientConfig);
                Assert.Throws<InvalidOperationException>(() => cacheConfig.FullAddress());
            }

            [Test]
            public void ShouldReturnIpAndPortFormatedAsInTest()
            {
                var config = new RedisClientConfig
                {
                    AddressName = "Test",
                    IpAddress = "127.0.0.1",
                    Password = "",
                    Port = "22"
                };

                Assert.True(string.CompareOrdinal("127.0.0.1:22",config.FullAddress()) == 0);
            }

        }
    }
}
