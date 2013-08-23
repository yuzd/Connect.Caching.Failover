using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.Architecture.Interfaces.Caching.Monitoring
{
    public interface ISentinelClientsManager
    {
        /// <summary>
        /// Returns the first or last active monitor available
        /// </summary>
        /// <returns>the first or last active sentinel</returns>
        ISentinelClient GetActiveSentinel();
        
        /// <summary>
        /// Update the list of sentinels that monitors a specified master
        /// </summary>
        void RefreshSentinels();
    }
}
