using System;

namespace AuthApp.Features.User;

public static class UserModule
{
    public static WebApplicationBuilder RegisterUserModule(this WebApplicationBuilder builder)
    {

        builder.Services.AddScoped<UserRepository>();
        builder.Services.AddScoped<UserService>();
        return builder;
    }
}
