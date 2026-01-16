using AuthApp.Common.RateLimit;
using Microsoft.AspNetCore.RateLimiting;
using StackExchange.Redis;

namespace AuthApp.Common.Middleware;

public sealed class RedisRateLimitMiddleware(RequestDelegate next, IConnectionMultiplexer redis)
{
    private const string RedisKeyPrefix = "ratelimit:v1";

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

        var windowSeconds = (long)policy.Window.TotalSeconds;
        var windowId = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / windowSeconds;

        var redisKey = $"{RedisKeyPrefix}:{attr.PolicyName}:{partition}:{windowId}";

        int result;
        try
        {
            result = (int)
                await _db.ScriptEvaluateAsync(
                    RedisScripts.FixedWindowEnforced,
                    [redisKey],
                    [windowSeconds, policy.PermitLimit]
                );
        }
        catch
        {
            await _next(context);
            return;
        }

        if (result == -1)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/json";
            context.Response.Headers.RetryAfter = windowSeconds.ToString();

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
