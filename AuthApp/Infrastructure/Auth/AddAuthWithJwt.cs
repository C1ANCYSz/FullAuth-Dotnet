using System.IdentityModel.Tokens.Jwt;
using System.Text;
using AuthApp.Common.Auth;
using AuthApp.Common.Constants;
using AuthApp.Config;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthApp.Infrastructure.Auth;

public static class AddAuth
{
    public static WebApplicationBuilder AddAuthWithJwt(this WebApplicationBuilder builder)
    {
        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

        using var provider = builder.Services.BuildServiceProvider();

        var jwt = provider.GetRequiredService<IOptions<JwtOptions>>().Value;

        var oauth = provider.GetRequiredService<IOptions<OAuthOptions>>().Value;

        builder
            .Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwt.ACCESS_TOKEN_SECRET)
                    ),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                };
            })
            .AddGoogle(
                "Google",
                options =>
                {
                    options.ClientId = oauth.Google.CLIENT_ID;
                    options.ClientSecret = oauth.Google.CLIENT_SECRET;
                    options.CallbackPath = oauth.Google.CALLBACK_URI;
                }
            );

        builder
            .Services.AddAuthorizationBuilder()
            .AddPolicy(
                AuthPolicies.Onboard,
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(JwtArributes.isOnboard, JwtArributes.trueValue);
                }
            )
            .AddPolicy(
                AuthPolicies.NotOnboard,
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(JwtArributes.isOnboard, JwtArributes.falseValue);
                }
            );

        return builder;
    }
}
