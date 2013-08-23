namespace SS.Architecture.Cache.Redis.Sentinel
{
    internal static class SentinelInfoFields
    {
        public static readonly string Name = "name";
        public static readonly string Ip = "ip";
        public static readonly string Port = "port";
        public static readonly string RunId = "runid";
        public static readonly string Flags = "flags";
        public static readonly string LastOkPingReply = "last-ok-ping-reply";
        public static readonly string LastPingReply = "last-ping-reply";
        public static readonly string InfoRefresh = "info-refresh";
        public static readonly string PendingCommands = "pending-commands";
        public static readonly string IsHostDown = "s-down-time";
        
        //Master Specific fields
        public static readonly string NumberOfSlaves = "num-slaves";
        public static readonly string NumberOfOtherSentinels = "num-other-sentinels";
        public static readonly string Quorum = "quorum";

        //Slaves Specific fields
        public static readonly string MasterLinkDownTime = "master-link-down-time";
        public static readonly string MasterLinkStatus = "master-link-status";
        public static readonly string MasterIp = "master-host";
        public static readonly string MasterPort = "master-port";
        public static readonly string SlavePriority = "slave-priority";
    }
}
