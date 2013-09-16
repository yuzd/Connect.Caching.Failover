using System;
using System.Collections.Generic;

namespace SS.Architecture.Interfaces.Caching.Monitoring
{
    public interface ISentinelClient
    {
        bool Ping();

        Tuple<string, int> GetMasterByAddressName(string addressName);
        IEnumerable<Tuple<string, int>> GetSlavesByAddressName(string addressName);

        void Reset(string addressName);
    }
}
