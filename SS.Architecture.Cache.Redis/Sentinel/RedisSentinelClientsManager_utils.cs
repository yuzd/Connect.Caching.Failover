using System;
using System.Threading;

namespace SS.Architecture.Cache.Redis.Sentinel
{
    public partial class RedisSentinelClientsManager
    {

        ~RedisSentinelClientsManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private int _disposeAttempts;
        protected virtual void Dispose(bool disposing)
        {
            if (Interlocked.Increment(ref _disposeAttempts) > 1) return;

            if (disposing)
            {
                // get rid of managed resources
            }

            try
            {
                // get rid of unmanaged resources
                for (var i = 0; i < _writeClients.Length; i++)
                {
                    Dispose(_writeClients[i]);
                }
                for (var i = 0; i < _readClients.Length; i++)
                {
                    Dispose(_readClients[i]);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error when trying to dispose of RedisSentinelClientsManager", ex);
            }
        }

        protected void Dispose(MonitoredRedisClient redisClient)
        {
            if (redisClient == null) return;
            try
            {
                redisClient.Dispose();
            }
            catch (Exception ex)
            {
                Logger.LogError(string.Format("Error when trying to dispose of MonitoredRedisClient to host {0}:{1}", redisClient.Host, redisClient.Port), ex);
            }
        }
    }
}
