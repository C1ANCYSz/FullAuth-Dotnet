namespace AuthApp.Common.RateLimit;

public static class RateLimitPolicies
{
    // Auth
    public const string AuthLogin = "auth:login";
    public const string AuthSignup = "auth:signup";

    public const string AuthRefresh = "auth:refresh";
    public const string AuthLogout = "auth:logout";

    // User
    public const string UserRead = "user:read";
    public const string UserWrite = "user:write";
    public const string UserSensitive = "user:sensitive";
}
