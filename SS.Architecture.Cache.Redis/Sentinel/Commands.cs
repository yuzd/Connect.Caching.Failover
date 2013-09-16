using ServiceStack.Text;

namespace SS.Architecture.Cache.Redis.Sentinel
{
    public static class Commands
    {
        public readonly static byte[] Sentinel = "sentinel".ToUtf8Bytes();
        public readonly static byte[] Masters = "masters".ToUtf8Bytes();
        public readonly static byte[] Slaves = "slaves".ToUtf8Bytes();
        public readonly static byte[] IsMasterDown = "is-master-down-by-addr".ToUtf8Bytes();
        public readonly static byte[] GetMasterAddress = "get-master-addr-by-name".ToUtf8Bytes();
        public readonly static byte[] Reset = "reset".ToUtf8Bytes();
    }
}
