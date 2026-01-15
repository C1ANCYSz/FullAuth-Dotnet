using System.IdentityModel.Tokens.Jwt;
using System.Text;
using AuthApp.Common.Constants;
using AuthApp.Config;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace AuthApp.Infrastructure.Auth;

public static class AddAuth
{
    public static WebApplicationBuilder AddAuthWithJwt(this WebApplicationBuilder builder)
    {
        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

        builder
            .Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var jwt = builder.Configuration.GetSection("JWT").Get<JwtOptions>()!;

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
            });

        builder
            .Services.AddAuthorizationBuilder()
            .AddPolicy(
                "OnboardedOnly",
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("is_onboard", JwtArributes.trueValue);
                }
            )
            .AddPolicy(
                "NotOnboardedOnly",
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("is_onboard", JwtArributes.falseValue);
                }
            );

        return builder;
    }
}
