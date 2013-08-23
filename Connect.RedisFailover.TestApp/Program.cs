using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SS.Architecture.Cache.Redis.Configuration;
using System.Configuration;
using SS.Architecture.Cache.Redis.Sentinel;
using SS.Architecture.Cache.Redis;
using System.Threading;

namespace Connect.RedisFailover.TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var cacheConfig = ((RedisCacheConfigSectionHandler)ConfigurationManager.GetSection("redisCacheConfigSection")).RedisCacheConfig;
            var logger = new SS.Architecture.Logging.ConsoleLogger();

            var redisSentinelManager = new RedisSentinelClientsManager(logger, cacheConfig);

            var serializer = new DefaultSerializer();

            var cacheClient = new RedisCacheClient(logger, serializer, cacheConfig, redisSentinelManager);


            var list = new[] { 1, };//2, 3, 4, 5, 6, 7, 8, 9, 10 };

            Parallel.ForEach(list, x =>
            {
                var keyValue = 0;
                while (true)
                {
                    var key = "Test" + x;
                    cacheClient.Set(key, "", keyValue.ToString());

                    var value = cacheClient.Get<string>(key, "");
                    Console.WriteLine("Key:{0} Value:{1}", key, value);
                    
                    keyValue++;
                    Thread.Sleep(1000);
                }
            });


            Console.Read();
        }
    }
}
