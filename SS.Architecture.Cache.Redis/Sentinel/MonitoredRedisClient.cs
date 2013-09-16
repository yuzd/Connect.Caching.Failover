using ServiceStack.Redis;

namespace SS.Architecture.Cache.Redis.Sentinel
{
    public class MonitoredRedisClient : RedisClient
    {
        public MonitoredRedisClient()
        {

        }

        public MonitoredRedisClient(string host)
            : base(host)
        {

        }

        public MonitoredRedisClient(string host, int port)
            : this(host, port, null)
        {

        }

        public MonitoredRedisClient(string host, int port, string password = null)
            : base(host, port, password)
        {

        }


        #region Multithreading management
        internal bool Active { get; set; }
        internal RedisSentinelClientsManager ClientsManager { get; set; }

        ~MonitoredRedisClient()
        {
            Dispose(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (ClientsManager != null)
            {
                ClientsManager.DisposeClient(this);
                return;
            }
            base.Dispose(disposing);
        } 
        #endregion
    }
}
