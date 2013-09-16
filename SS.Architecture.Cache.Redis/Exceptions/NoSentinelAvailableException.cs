using ServiceStack.Redis;

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
