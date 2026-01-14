using System;

namespace AuthApp.Common.Middleware;

using AuthApp.Common.RateLimit;
using Microsoft.AspNetCore.RateLimiting;
using StackExchange.Redis;

public sealed class RedisRateLimitMiddleware(RequestDelegate next, IConnectionMultiplexer redis)
{
    private readonly RequestDelegate _next = next;
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var attr = endpoint?.Metadata.GetMetadata<EnableRateLimitingAttribute>();

        if (attr is null)
        {
            await _next(context);
            return;
        }
        var policyName = attr.PolicyName;

        if (string.IsNullOrWhiteSpace(policyName))
        {
            await _next(context);
            return;
        }

        if (!AuthRateLimitConfig.Policies.TryGetValue(policyName, out var policy))
        {
            await _next(context);
            return;
        }

        var partition = RateLimitPartition.Resolve(context);

        var windowSeconds = (long)policy.Window.TotalSeconds;
        var windowId = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / windowSeconds;

        var redisKey = $"ratelimit:{attr.PolicyName}:{partition}:{windowId}";

        var count = (int)
            await _db.ScriptEvaluateAsync(
                RedisScripts.FixedWindow,
                [redisKey],
                [windowSeconds, policy.PermitLimit]
            );

        if (count > policy.PermitLimit)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/json";

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
}
