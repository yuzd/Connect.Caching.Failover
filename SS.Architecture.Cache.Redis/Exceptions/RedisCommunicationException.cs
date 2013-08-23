using ServiceStack.Redis;
using System;

namespace SS.Architecture.Cache.Redis.Exceptions
{
    public class RedisCommunicationException : RedisException
    {
        public RedisCommunicationException(string message)
            : base(message)
        {

        }

        public RedisCommunicationException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
