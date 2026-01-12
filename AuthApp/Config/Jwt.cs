using System;
using System.ComponentModel.DataAnnotations;

namespace AuthApp.Config;

public sealed class JwtOptions
{
    [Required]
    public required string ACCESS_TOKEN_SECRET { get; init; }

    [Required]
    public required string REFRESH_TOKEN_SECRET { get; init; }

    [Required]
    public required TimeSpan ACCESS_TOKEN_EXPIRY { get; init; }

    [Required]
    public required TimeSpan REFRESH_TOKEN_EXPIRY { get; init; }
}

public static class RegisterJwt
{
    public static WebApplicationBuilder RegisterJwtOptions(this WebApplicationBuilder builder)
    {
        builder
            .Services.AddOptions<JwtOptions>()
            .Bind(builder.Configuration.GetSection("JWT"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return builder;
    }
}
