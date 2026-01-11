using System;
using AuthApp.Features.Auth.DTOs;
using FluentValidation;

namespace AuthApp.Features.Auth.Validators;

public class LoginValidator : AbstractValidator<LoginDto>
{

    public LoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);

    }

}
