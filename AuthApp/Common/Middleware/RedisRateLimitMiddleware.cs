using AuthApp.Common.RateLimit;
using Microsoft.AspNetCore.RateLimiting;
using StackExchange.Redis;

namespace AuthApp.Common.Middleware;

public sealed class RedisRateLimitMiddleware(RequestDelegate next, IConnectionMultiplexer redis)
{
    private const string RedisKeyPrefix = "ratelimit:v2";

    private readonly RequestDelegate _next = next;
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var attr = endpoint?.Metadata.GetMetadata<EnableRateLimitingAttribute>();

        if (attr?.PolicyName is null)
        {
            await _next(context);
            return;
        }

        if (!AuthRateLimitConfig.Policies.TryGetValue(attr.PolicyName, out var policy))
        {
            await _next(context);
            return;
        }

        var partition = ResolvePartition(context);
        if (partition is null)
        {
            await _next(context);
            return;
        }

        var redisKey = $"{RedisKeyPrefix}:{attr.PolicyName}:{partition}";
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var capacity = policy.PermitLimit;
        var refillRatePerSecond = capacity / policy.Window.TotalSeconds;

        int result;
        try
        {
            result = (int)
                await _db.ScriptEvaluateAsync(
                    RedisScripts.TokenBucket,
                    [redisKey],
                    [capacity, refillRatePerSecond, now]
                );
        }
        catch
        {
            // fail open
            await _next(context);
            return;
        }

        if (result == -1)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/json";
            context.Response.Headers.RetryAfter = ((int)policy.Window.TotalSeconds).ToString();

            await context.Response.WriteAsync(
                $$"""
                {
                  "error": "rate_limited",
                  "message": "{{policy.Message}}"
                }
                """
            );
            return;
        }

        await _next(context);
    }

    private static string? ResolvePartition(HttpContext context)
    {
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst("sub")?.Value;
            if (!string.IsNullOrWhiteSpace(userId))
                return $"user:{userId}";
        }

        var ip = RateLimitPartition.ResolveForKey(context);
        if (!string.IsNullOrWhiteSpace(ip))
            return $"ip:{ip}";

        return null;
    }
}
