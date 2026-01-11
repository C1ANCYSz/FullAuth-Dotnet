using System;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace AuthApp.Infrastructure;

public static class FluentValidation
{
    public static WebApplicationBuilder AddFluentValidation(this WebApplicationBuilder builder)
    {
        builder.Services.AddFluentValidationAutoValidation()
        .AddValidatorsFromAssemblyContaining<Program>();

        return builder;
    }
}
