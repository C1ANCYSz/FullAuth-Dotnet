using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthApp.Common.Constants;
using AuthApp.Common.Errors;

namespace AuthApp.Common.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value =
            user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? throw new UnauthorizedException("User ID claim missing");

        return Guid.Parse(value);
    }

    public static int GetTokenVersion(this ClaimsPrincipal user)
    {
        var value =
            user.FindFirst("token_version")?.Value
            ?? throw new UnauthorizedException("Token version missing");

        return int.Parse(value);
    }

    public static bool IsOnboarded(this ClaimsPrincipal user)
    {
        var value = user.FindFirst(JwtArributes.isOnboard)?.Value;
        return value is "true";
    }
}
