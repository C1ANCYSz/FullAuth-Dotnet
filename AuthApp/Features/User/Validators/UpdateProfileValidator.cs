using System;
using AuthApp.Features.User.DTOs;
using FluentValidation;

namespace AuthApp.Features.User.Validators;

public class UpdateProfileValidator : AbstractValidator<UpdateProfileDto>
{
    public UpdateProfileValidator()
    {
        RuleFor(x => x.Bio).NotEmpty().WithMessage("Bio is required");
    }
}
