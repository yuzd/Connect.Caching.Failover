using System.Net;
using ServiceStack.Text;
using SS.Architecture.Interfaces.Caching.Monitoring;
using System;
using System.Linq;
using System.Collections.Generic;

namespace SS.Architecture.Cache.Redis.Sentinel
{
    public class RedisSentinelClient : MonitoredRedisClient, ISentinelClient
    {
        public RedisSentinelClient()
        {

        }

        public RedisSentinelClient(string host)
            : base(host)
        {

        }

        public RedisSentinelClient(string host, int port)
            : this(host, port, null)
        {

        }

        public RedisSentinelClient(string host, int port, string password = null)
            : base(host, port, password)
        {

        }

        public Tuple<string, int> GetMasterByAddressName(string addressName)
        {
            var result = SendExpectMultiData(Commands.Sentinel, Commands.GetMasterAddress, addressName.ToUtf8Bytes());
            if (result.Length == 2)
            {
                var ip = result[0].FromUtf8Bytes();
                var port = Convert.ToInt32(result[1].FromUtf8Bytes());
                return new Tuple<string, int>(ip, port);
            }
            return null;
        }

        public IEnumerable<Tuple<string, int>> GetSlavesByAddressName(string addressName)
        {
            //Get Slaves List
            var byResult = SendExpectDeeplyNestedMultiData(Commands.Sentinel, Commands.Slaves, addressName.ToUtf8Bytes());

            var slaves = ParseSentinelResult(byResult);
            //From the List Remove All Inactive slaves 
            //Return Ip + port ordered by slave priority
            return slaves.Where(x =>
                {
                    if (x.ContainsKey(SentinelInfoFields.IsHostDown)) { return false; }

                    var flags = x[SentinelInfoFields.Flags];
                    if (flags.Contains(SentinelInfoValues.HostDisconnected) ||
                        flags.Contains(SentinelInfoValues.HostDown))
                    {
                        return false;
                    }
                    
                    //Suppress any corrupted data coming from sentinel where slave ip comes up as loopback.
                    if (IPAddress.Loopback.Equals(IPAddress.Parse(x[SentinelInfoFields.Ip]))) { return false; }

                    if (x[SentinelInfoFields.MasterLinkStatus] != SentinelInfoValues.Ok)
                    {
                        return false;
                    }
                    return true;
                })
                .OrderBy(x => Convert.ToInt32(x[SentinelInfoFields.SlavePriority]))
                .Select(x => new Tuple<string, int>(x[SentinelInfoFields.Ip], Convert.ToInt32(x[SentinelInfoFields.Port])));
        }

        public bool IsMasterDown(string addressName)
        {
            var addressInfo = GetMasterByAddressName(addressName);
            return addressInfo != null && IsMasterDown(addressInfo.Item1, addressInfo.Item2);
        }

        public bool IsMasterDown(string ip, int port)
        {
            var result = SendExpectMultiData(Commands.Sentinel, Commands.IsMasterDown, ip.ToUtf8Bytes(), port.ToUtf8Bytes());
            return Convert.ToInt32(result[0].FromUtf8Bytes()) == 0;
        }

        public void Reset(string addressName)
        {
            throw new NotImplementedException();
        }
        
        private IEnumerable<Dictionary<string, string>> ParseSentinelResult(IEnumerable<object> stream)
        {
            var result = new List<Dictionary<string, string>>();

            foreach (object[] item in stream)
            {
                var infos = new Dictionary<string, string>();
                for (var i = 0; i < item.Length; i++)
                {
                    var key = ((byte[])item[i]).FromUtf8Bytes();
                    var value = ((byte[])item[++i]).FromUtf8Bytes();
                    infos.Add(key, value);
                }
                result.Add(infos);
            }

            return result;
        }
        
        protected override void Dispose(bool disposing)
        {
            if (ClientsManager != null)
            {
                ClientsManager.DisposeSentinelClient(this);
                return;
            }
            base.Dispose(disposing);
        }
    }
}
