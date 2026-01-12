namespace AuthApp.Features.Auth;

public static class AuthModule
{
    public static WebApplicationBuilder RegisterAuthModule(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<AuthService>();
        return builder;
    }
}
