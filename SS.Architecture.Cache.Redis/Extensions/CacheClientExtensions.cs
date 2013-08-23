using System;
using SS.Architecture.Interfaces.Caching;
using System.Net;
using System.Text.RegularExpressions;

namespace SS.Architecture.Cache.Redis.Extensions
{
    public static class CacheClientExtensions
    {
        private static readonly Regex PortNumber = new Regex(@"^(6553[0-5]|655[0-2]\d|65[0-4]\d\d|6[0-4]\d{3}|[1-5]\d{4}|[1-9]\d{0,3}|0)$");

        public static bool IsRedisHostConfigValid(this ICacheClientConfig client)
        {
            IPAddress add;
            return client != null && IPAddress.TryParse(client.IpAddress, out add) && PortNumber.IsMatch(client.Port);
        }

        public static bool HasAddressName(this ICacheClientConfig client)
        {
            return client != null && !string.IsNullOrWhiteSpace(client.AddressName);
        }

        public static bool RequirePassword(this ICacheClientConfig client)
        {
            return client != null && !string.IsNullOrEmpty(client.Password);
        }

        public static string FullAddress(this ICacheClientConfig client)
        {
            if (client == null)
                throw new InvalidOperationException("Cache client config cannot be null.");
            return string.Format("{0}:{1}", client.IpAddress, client.Port);
        }
    }
}
