using System;
using AuthApp.Features.Auth.DTOs;
using FluentValidation;

namespace AuthApp.Features.Auth.Validators;

public sealed class OAuthValidator : AbstractValidator<OAuthDto>
{
    public OAuthValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .Must(BeValidJwtFormat)
            .WithMessage("ID token must be a valid JWT");
    }

    private static bool BeValidJwtFormat(string token)
    {
        var parts = token.Split('.');
        if (parts.Length != 3)
            return false;

        return parts.All(IsBase64Url);
    }

    private static bool IsBase64Url(string value)
    {
        return value.Length > 0 && value.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_');
    }
}
