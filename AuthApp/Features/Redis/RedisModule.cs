namespace AuthApp.Features.Redis;

public static class RedisModule
{
    public static WebApplicationBuilder RegisterRedisModule(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<RedisService>();

        return builder;
    }
}
