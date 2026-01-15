using System;
using AuthApp.Config;
using Microsoft.Extensions.Options;

namespace AuthApp.Infrastructure.Email;

public static class RegisterSmtp
{
    public static WebApplicationBuilder RegisterSmtpClient(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));

        builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

        return builder;
    }
}
