using System.ComponentModel.DataAnnotations;

namespace AuthApp.Config;

public sealed class OAuthOptions
{
    [Required]
    public GoogleOAuthOptions Google { get; init; } = default!;
}

public sealed class GoogleOAuthOptions
{
    [Required]
    public required string CLIENT_ID { get; init; }

    [Required]
    public required string CLIENT_SECRET { get; init; }

    [Required]
    public required string CALLBACK_URI { get; init; }
}

public static class OAuthOptionsRegistration
{
    public static WebApplicationBuilder RegisterOAuthOptions(this WebApplicationBuilder builder)
    {
        builder
            .Services.AddOptions<OAuthOptions>()
            .Bind(builder.Configuration.GetSection("OAUTH"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return builder;
    }
}
