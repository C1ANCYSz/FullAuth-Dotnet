
using System.ComponentModel.DataAnnotations;

namespace AuthApp.Config;

public sealed class Env
{

    [Required] public string DB_CONNECTION_STRING { get; init; } = default!;


}


public static class RegisterEnv
{
    public static WebApplicationBuilder RegisterEnvVariables(this WebApplicationBuilder builder)
    {

        builder.Services.AddOptions<Env>()
        .Bind(builder.Configuration.GetSection("ENV"))
        .ValidateDataAnnotations()
        .ValidateOnStart();


        return builder;
    }
}