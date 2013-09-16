using System.Collections.Generic;
using ServiceStack.Redis;
using System;
using System.Linq;
using System.Threading;
using SS.Architecture.Cache.Redis.Extensions;

namespace SS.Architecture.Cache.Redis.Sentinel
{
    public partial class RedisSentinelClientsManager
    {
        private const string PoolTimeoutError =
                   "Redis Sentinel Timeout expired. The timeout period elapsed prior to obtaining a connection from the pool. This may have occurred because all pooled connections were in use.";

        public readonly int PoolTimeout;
        public const int RecheckPoolAfterMs = 100;
        
        private MonitoredRedisClient[] _readClients = new MonitoredRedisClient[0];
        private MonitoredRedisClient[] _writeClients = new MonitoredRedisClient[0];

        protected int WritePoolIndex;
        protected int ReadPoolIndex;

        private void OnStart()
        {
             if (_writeClients.Length > 0 || _readClients.Length > 0)
                throw new InvalidOperationException("Pool has already been started");

            _writeClients = new MonitoredRedisClient[Config.MaxWritePoolSize];
            WritePoolIndex = 0;

            _readClients = new MonitoredRedisClient[Config.MaxReadPoolSize];
            ReadPoolIndex = 0;
        }

        public IRedisClient GetRedisClient(string addressName)
        {
            Tuple<string, int> masterLocation;

            using (var sentinel = GetActiveSentinel() as RedisSentinelClient)
            {
                masterLocation = sentinel.GetMasterByAddressName(addressName);    
            }
            
            lock (_writeClients)
            {
                MonitoredRedisClient inActiveClient;

                if (!_writeClients.Any())
                {
                    inActiveClient = masterLocation.ToRedisClient();
                    inActiveClient.ClientsManager = this;
                    inActiveClient.Active = true;
                    _writeClients[WritePoolIndex] = inActiveClient;
                    WritePoolIndex++;
                    return inActiveClient;
                }

                while ((inActiveClient = GetInActiveWriteClient(masterLocation)) == null)
                {
                    if (PoolTimeout > 0)
                    {
                        // wait for a connection, cry out if made to wait too long
                        if (!Monitor.Wait(_writeClients, PoolTimeout))
                            throw new TimeoutException(PoolTimeoutError);
                    }
                    else
                        Monitor.Wait(_writeClients, RecheckPoolAfterMs);
                }

                inActiveClient.Active = true;
                return inActiveClient;
            }
        }

        private MonitoredRedisClient GetInActiveWriteClient(Tuple<string, int> masterInfos)
        {
            var emptySlot = -1;

            for (int i = 0; i < WritePoolIndex; i++)
            {
                //use free slot to add new instance if needed.
                if(_writeClients[i] == null)  { emptySlot = i; continue; } 

                if (_writeClients[i].Host == masterInfos.Item1 && _writeClients[i].Port == masterInfos.Item2)
                {
                    if (_writeClients[i].Active) return null;
                    if (!_writeClients[i].HadExceptions) return _writeClients[i];    

                    //if previous exception reset connection.
                    _writeClients[i].Dispose();
                    var cli = masterInfos.ToRedisClient();
                    cli.ClientsManager = this;
                    _writeClients[i] = cli;

                    return _writeClients[i];
                }
            }

            var inActiveClient = masterInfos.ToRedisClient();
            inActiveClient.ClientsManager = this;
            _writeClients[emptySlot >= 0 ? emptySlot : WritePoolIndex] = inActiveClient;
            if (emptySlot >= 0) WritePoolIndex++;
            return inActiveClient;
        }

        public IRedisClient GetReadOnlyRedisClient(string addressName)
        {
            Tuple<string, int>[] slavesLocations;

            using (var sentinel = GetActiveSentinel() as RedisSentinelClient)
            {
                slavesLocations = sentinel.GetSlavesByAddressName(addressName).ToArray();
            }
            
            if (!slavesLocations.Any())
            {
                return GetRedisClient(addressName);
            }
            
            lock (_readClients)
            {
                MonitoredRedisClient inActiveClient;

                if (!_readClients.Any())
                {
                    inActiveClient = slavesLocations.First().ToRedisClient();
                    inActiveClient.ClientsManager = this;
                    inActiveClient.Active = true;
                    _readClients[ReadPoolIndex] = inActiveClient;
                    ReadPoolIndex++;
                    return inActiveClient;
                }

                while ((inActiveClient = GetInActiveReadClient(slavesLocations.ToArray())) == null)
                {
                    if (PoolTimeout > 0)
                    {
                        // wait for a connection, cry out if made to wait too long
                        if (!Monitor.Wait(_readClients, PoolTimeout))
                            throw new TimeoutException(PoolTimeoutError);
                    }
                    else
                        Monitor.Wait(_readClients, RecheckPoolAfterMs);
                }

                inActiveClient.Active = true;
                return inActiveClient;
            }
        }

        private MonitoredRedisClient GetInActiveReadClient(Tuple<string, int>[] slavesInfos)
        {
            var emptySlot = -1;

            for (var i = 0; i < slavesInfos.Length; i++)
            {
                var hostBusy = false;

                for (var j = 0; j < ReadPoolIndex; j++)
                {
                    if (_readClients[j] == null) { emptySlot = i; continue; }

                    if (_readClients[j].Host == slavesInfos[i].Item1 && _readClients[j].Port == slavesInfos[i].Item2)
                    {
                        if (_readClients[j].Active) { hostBusy = true; break; }
                        if (!_readClients[j].HadExceptions) { return _readClients[i]; }

                        _readClients[j].Dispose();
                        var client = slavesInfos[i].ToRedisClient();
                        client.ClientsManager = this;
                        _readClients[j] = client;
                        return client;
                    }
                }

                if (!hostBusy)
                {
                    var client = slavesInfos[i].ToRedisClient();
                    client.ClientsManager = this;

                    _readClients[emptySlot < 0 ? ReadPoolIndex : emptySlot] = client;
                    if (emptySlot < 0) ReadPoolIndex++;
                    return client;
                }
            }
                
            return null;
        }

        public void DisposeClient(MonitoredRedisClient client)
        {
            lock (_readClients)
            {
                for (var i = 0; i < _readClients.Length; i++)
                {
                    var readClient = _readClients[i];
                    if (client != readClient) continue;
                    client.Active = false;
                    Monitor.PulseAll(_readClients);
                    return;
                }
            }

            lock (_writeClients)
            {
                for (var i = 0; i < _writeClients.Length; i++)
                {
                    var writeClient = _writeClients[i];
                    if (client != writeClient) continue;
                    client.Active = false;
                    Monitor.PulseAll(_writeClients);
                    return;
                }
            }

            //Client not found in any pool, pulse both pools.
            lock (_readClients)
                Monitor.PulseAll(_readClients);
            lock (_writeClients)
                Monitor.PulseAll(_writeClients);

        }
        
    }
}
