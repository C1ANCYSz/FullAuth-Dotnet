
using FluentValidation;
using AuthApp.Common.Exeptions;

namespace AuthApp.Middlewares;

public sealed class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            if (context.Response.HasStarted) throw;

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await WriteJson(context, new
            {
                errors = ex.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    )
            });
        }
        catch (DomainException ex)
        {
            if (context.Response.HasStarted) throw;

            context.Response.StatusCode = ex.StatusCode;
            await WriteJson(context, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted) throw;

            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await WriteJson(context, new { error = "Internal server error" });
        }
    }


    private static async Task WriteJson(HttpContext context, object payload)
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(payload);
    }
}
