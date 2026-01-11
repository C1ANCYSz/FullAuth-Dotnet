using System;

namespace AuthApp.Features.Jwt;

public static class Jwtmodule
{
    public static WebApplicationBuilder RegisterJwtModule(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<JwtService>();
        return builder;
    }
}
