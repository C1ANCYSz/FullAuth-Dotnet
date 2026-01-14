using Microsoft.AspNetCore.RateLimiting;

namespace AuthApp.Common.RateLimit;

internal sealed record FixedWindowPolicy(TimeSpan Window, int PermitLimit, string Message);

internal static class AuthRateLimitConfig
{
    public static readonly IReadOnlyDictionary<string, FixedWindowPolicy> Policies = new Dictionary<
        string,
        FixedWindowPolicy
    >
    {
        [RateLimitPolicies.AuthLogin] = new(
            Window: TimeSpan.FromMinutes(1),
            PermitLimit: 5,
            Message: "Too many login attempts. Try again in 1 minute."
        ),

        [RateLimitPolicies.AuthSignup] = new(
            Window: TimeSpan.FromMinutes(10),
            PermitLimit: 3,
            Message: "Too many signup attempts. Please try again later."
        ),

        [RateLimitPolicies.AuthOtp] = new(
            Window: TimeSpan.FromMinutes(1),
            PermitLimit: 3,
            Message: "OTP requests are limited. Try again shortly."
        ),
    };
}

public static class PublicAuthRateLimit
{
    public static WebApplicationBuilder RegisterRateLimits(this WebApplicationBuilder builder)
    {
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, token) =>
            {
                var policyName = context
                    .HttpContext.GetEndpoint()
                    ?.Metadata.GetMetadata<EnableRateLimitingAttribute>()
                    ?.PolicyName;

                var message =
                    policyName != null
                    && AuthRateLimitConfig.Policies.TryGetValue(policyName, out var policy)
                        ? policy.Message
                        : "Too many requests.";

                context.HttpContext.Response.ContentType = "application/json";

                await context.HttpContext.Response.WriteAsync(
                    $$"""
                    {
                      "error": "rate_limited",
                      "message": "{{message}}"
                    }
                    """,
                    token
                );
            };

            foreach (var (name, policy) in AuthRateLimitConfig.Policies)
            {
                options.AddFixedWindowLimiter(
                    name,
                    opt =>
                    {
                        opt.Window = policy.Window;
                        opt.PermitLimit = policy.PermitLimit;
                        opt.QueueLimit = 0;
                    }
                );
            }
        });

        return builder;
    }
}
