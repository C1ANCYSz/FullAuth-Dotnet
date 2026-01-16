namespace AuthApp.Common.RateLimit;

internal static class RedisScripts
{
    public const string TokenBucket = """
        -- KEYS[1] = bucket key
        -- ARGV[1] = capacity
        -- ARGV[2] = refill_rate_per_second
        -- ARGV[3] = now (unix seconds)

        local capacity = tonumber(ARGV[1])
        local rate = tonumber(ARGV[2])
        local now = tonumber(ARGV[3])

        local data = redis.call("HMGET", KEYS[1], "tokens", "ts")
        local tokens = tonumber(data[1])
        local last_ts = tonumber(data[2])

        if not tokens then
            tokens = capacity
            last_ts = now
        end

        local delta = math.max(0, now - last_ts)
        tokens = math.min(capacity, tokens + delta * rate)

        if tokens < 1 then
            redis.call("HMSET", KEYS[1], "tokens", tokens, "ts", now)
            redis.call("EXPIRE", KEYS[1], math.ceil(capacity / rate))
            return -1
        end

        tokens = tokens - 1

        redis.call("HMSET", KEYS[1], "tokens", tokens, "ts", now)
        redis.call("EXPIRE", KEYS[1], math.ceil(capacity / rate))

        return tokens
        """;

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
