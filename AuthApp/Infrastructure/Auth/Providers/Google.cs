using AuthApp.Common.Auth;
using AuthApp.Config;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace AuthApp.Infrastructure.Auth.Providers;

public sealed class GoogleAuthProvider(IOptions<OAuthOptions> options) : IOAuthProvider
{
    private readonly OAuthOptions _oauth = options.Value;

    public AuthProvider Provider => AuthProvider.GOOGLE;

    public async Task<OAuthIdentity> ValidateAndGetEmailAsync(string idToken)
    {
        GoogleJsonWebSignature.Payload payload;

        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(
                idToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [_oauth.Google.CLIENT_ID],
                }
            );
        }
        catch
        {
            throw new InvalidOperationException("Invalid Google token");
        }

        if (string.IsNullOrWhiteSpace(payload.Email))
            throw new InvalidOperationException("Google account has no email");

        return new OAuthIdentity(payload.Subject, payload.Email);
    }
}
