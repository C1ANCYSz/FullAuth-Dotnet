namespace AuthApp.Common.RateLimit;

public static class RateLimitPolicies
{
    public const string AuthLogin = "auth:login";
    public const string AuthSignup = "auth:signup";
    public const string AuthOtp = "auth:otp";
}
