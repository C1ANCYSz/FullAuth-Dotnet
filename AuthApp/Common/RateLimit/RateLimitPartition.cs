using System;

namespace AuthApp.Common.RateLimit;

internal static class RateLimitPartition
{
    public static string Resolve(HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress;

        if (ip is null)
            return "unknown";

        if (ip.IsIPv4MappedToIPv6)
            ip = ip.MapToIPv4();

        if (ip.Equals(System.Net.IPAddress.IPv6Loopback))
            return "127.0.0.1";

        return ip.ToString();
    }
}
