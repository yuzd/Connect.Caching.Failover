
namespace SS.Architecture.Interfaces.Caching
{
    public interface ICacheClientConfig
    {
        string IpAddress { get; set; }
        string Port { get; set; }
        string Password { get; set; }
        string AddressName { get; set; }
    }
}
