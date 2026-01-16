using System;

namespace AuthApp.Infrastructure.Auth.Providers;

public static class RegisterProviders
{
    public static WebApplicationBuilder RegisterOAuthProviders(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IOAuthProvider, GoogleAuthProvider>();

        return builder;
    }
}
