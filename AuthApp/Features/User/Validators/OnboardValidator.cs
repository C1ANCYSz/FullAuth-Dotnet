using System;
using AuthApp.Features.User.DTOs;
using FluentValidation;

namespace AuthApp.Features.User.Validators;

public class OnboardValidator : AbstractValidator<OnboardDto>
{
    public OnboardValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(x => x.Dob).NotEmpty().WithMessage("Date of birth is required");
    }
}
