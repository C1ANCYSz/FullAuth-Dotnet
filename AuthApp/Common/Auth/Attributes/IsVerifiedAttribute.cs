namespace AuthApp.Common.Auth.Attributes;

using AuthApp.Common.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class IsVerifiedAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var skip = context
            .ActionDescriptor.EndpointMetadata.OfType<SkipIsVerifiedAttribute>()
            .Any();

        if (skip)
            return;

        var user = context.HttpContext.User;

        if (user.Identity?.IsAuthenticated != true)
            return;

        var isVerified = user.FindFirst(JwtArributes.isVerified)?.Value == JwtArributes.trueValue;

        if (!isVerified)
        {
            context.Result = new ObjectResult(new { error = "Please verify your email first" })
            {
                StatusCode = StatusCodes.Status403Forbidden,
            };
        }
    }
}
