namespace AuthApp.Common.Auth.Attributes;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireOnboardAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var skip = context
            .ActionDescriptor.EndpointMetadata.OfType<SkipOnboardCheckAttribute>()
            .Any();

        if (skip)
            return;

        var user = context.HttpContext.User;

        if (user.Identity?.IsAuthenticated != true)
            return;

        var isOnboard = user.FindFirst("is_onboard")?.Value == "true";

        if (!isOnboard)
        {
            context.Result = new ObjectResult(new { error = "Please complete onboarding first" })
            {
                StatusCode = StatusCodes.Status409Conflict,
            };
        }
    }
}
