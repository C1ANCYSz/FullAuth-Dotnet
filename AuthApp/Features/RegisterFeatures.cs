using System;
using AuthApp.Features.Auth;
using AuthApp.Features.Jwt;
using AuthApp.Features.User;

namespace AuthApp.Features;

public static class RegisterAppFeatures
{
    public static WebApplicationBuilder RegisterFeatures(this WebApplicationBuilder builder)
    {
        builder.RegisterUserModule();
        builder.RegisterAuthModule();
        builder.RegisterJwtModule();
        return builder;
    }

}
