using AuthApp.Common.Auth;

namespace AuthApp.Infrastructure.Auth.Providers;

public interface IOAuthProvider
{
    AuthProvider Provider { get; }
    Task<OAuthIdentity> ValidateAndGetEmailAsync(string token);
}
