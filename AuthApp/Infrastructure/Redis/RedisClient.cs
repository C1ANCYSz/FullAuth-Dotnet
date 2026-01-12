using System;
using AuthApp.Config;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
namespace AuthApp.Infrastructure.Redis;

public static class RedisClient
{
    public static WebApplicationBuilder RegisterRedisClient(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
          {
              var env = sp.GetRequiredService<IOptions<Env>>().Value;

              var options = ConfigurationOptions.Parse(env.REDIS_CONNECTION_STRING);
              options.AbortOnConnectFail = false;
              options.ConnectRetry = 5;
              options.ReconnectRetryPolicy = new ExponentialRetry(1000);
              return ConnectionMultiplexer.Connect(options);
          });

        return builder;
    }
}
