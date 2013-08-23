using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Architecture.Cache.Redis.Exceptions
{
    public class NoSentinelAvailableException : RedisException
    {
        public NoSentinelAvailableException()
            :base("No Active Sentinel Found. Please ensure that sentinels are running.")
        {
            
        }
    }
}
