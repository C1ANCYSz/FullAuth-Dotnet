namespace AuthApp.Common.RateLimit;

internal static class RedisScripts
{
    public const string FixedWindowEnforced = """
        local current = redis.call("INCR", KEYS[1])
        if current == 1 then
            redis.call("EXPIRE", KEYS[1], ARGV[1])
        end

        if current > tonumber(ARGV[2]) then
            return -1
        end

        return current
        """;

    public const string ConsumeIfEquals = """
        local actual = redis.call("GET", KEYS[1])
        if not actual then
            return 0
        end

        if actual == ARGV[1] then
            redis.call("DEL", KEYS[1])
            return 1
        end

        return 0
        """;

    public const string ConsumeValue = """
        local val = redis.call("GET", KEYS[1])
        if not val then
            return nil
        end

        redis.call("DEL", KEYS[1])
        return val
        """;

    public const string IncrementWithLimit = """
        local current = redis.call("INCR", KEYS[1])
        if current == 1 then
            redis.call("EXPIRE", KEYS[1], ARGV[2])
        end
        if current > tonumber(ARGV[1]) then
            return -1
        end
        return current
        """;
}
