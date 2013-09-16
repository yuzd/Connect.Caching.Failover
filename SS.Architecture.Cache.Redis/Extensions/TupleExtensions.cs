using SS.Architecture.Cache.Redis.Sentinel;
using System;

namespace SS.Architecture.Cache.Redis.Extensions
{
    public static class TupleExtensions
    {
        public static MonitoredRedisClient ToRedisClient(this Tuple<string, int> tuple)
        {
            return new MonitoredRedisClient(tuple.Item1, tuple.Item2);
        }
    }
}
