using System;


using AuthApp.Config;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using AuthApp.Common.Exceptions;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;

namespace AuthApp.Features.Jwt;

public sealed class JwtService(IOptions<JwtOptions> options)
{
    private readonly JwtOptions _jwt = options.Value;

    public string GenerateAccessToken(Guid userId, bool? isOnboard = null)
    {
        var claims = new List<Claim>
    {
        new(JwtRegisteredClaimNames.Sub, userId.ToString())
    };

        if (isOnboard.HasValue)
        {
            claims.Add(new Claim(
                "is_onboard",
                isOnboard.Value ? "true" : "false"
            ));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwt.ACCESS_TOKEN_SECRET)
        );

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
            new Claim("token_version", tokenVersion.ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwt.REFRESH_TOKEN_SECRET)
        );

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
                    ClockSkew = TimeSpan.Zero
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

    public Guid GetUserId(ClaimsPrincipal principal)
    {
        var sub = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new InvalidTokenException("Token missing sub claim");

        return Guid.Parse(sub);
    }

    public int GetTokenVersion(ClaimsPrincipal principal)
    {
        var value = principal.FindFirst("token_version")?.Value
            ?? throw new InvalidTokenException("Unauthorized Token");

        return int.Parse(value);
    }



}