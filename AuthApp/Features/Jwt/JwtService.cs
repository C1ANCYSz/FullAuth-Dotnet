using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthApp.Common.Constants;
using AuthApp.Common.Errors;
using AuthApp.Config;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthApp.Features.Jwt;

public sealed class JwtService(IOptions<JwtOptions> options)
{
    private readonly JwtOptions _jwt = options.Value;

    public string GenerateAccessToken(Guid userId, bool isOnboard, bool isVerified)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(
                JwtArributes.isOnboard,
                isOnboard ? JwtArributes.trueValue : JwtArributes.falseValue
            ),
            new Claim(
                JwtArributes.isVerified,
                isVerified ? JwtArributes.trueValue : JwtArributes.falseValue
            ),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.ACCESS_TOKEN_SECRET));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.Add(_jwt.ACCESS_TOKEN_EXPIRY),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken(Guid userId, int tokenVersion)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim("token_version", tokenVersion.ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.REFRESH_TOKEN_SECRET));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.Add(_jwt.REFRESH_TOKEN_EXPIRY),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal? VerifyRefreshToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();

        try
        {
            return handler.ValidateToken(
                token,
                new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(_jwt.REFRESH_TOKEN_SECRET)
                    ),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                },
                out _
            );
        }
        catch
        {
            return null;
        }
    }

    public string? GetBearerTokenFromHeaders(HttpRequest request)
    {
        var header = request.Headers.Authorization.FirstOrDefault();
        if (header is null || !header.StartsWith("Bearer "))
            return null;

        return header["Bearer ".Length..];
    }
}
