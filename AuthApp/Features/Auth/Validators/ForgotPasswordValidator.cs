using System;
using AuthApp.Features.Auth.DTOs;
using FluentValidation;

namespace AuthApp.Features.Auth.Validators;

public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordDto>
{
    public ForgotPasswordValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
