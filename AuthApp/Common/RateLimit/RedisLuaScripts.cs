using System;

namespace AuthApp.Common.RateLimit;

internal static class RedisScripts
{
    public const string FixedWindow = """
        local current = redis.call("INCR", KEYS[1])
        if current == 1 then
            redis.call("EXPIRE", KEYS[1], ARGV[1])
        end
        return current
        """;
}
