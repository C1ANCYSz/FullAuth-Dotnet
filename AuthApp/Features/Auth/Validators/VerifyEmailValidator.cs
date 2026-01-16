using System;
using AuthApp.Features.Auth.DTOs;
using FluentValidation;

namespace AuthApp.Features.Auth.Validators;

public class VerifyEmailValidator : AbstractValidator<VerifyEmailDto>
{
    public VerifyEmailValidator()
    {
        RuleFor(x => x.Code).NotEmpty().Length(6);
    }
}
