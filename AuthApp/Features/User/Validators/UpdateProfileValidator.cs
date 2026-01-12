using System;
using AuthApp.Features.User.DTOs;
using FluentValidation;

namespace AuthApp.Features.User.Validators;

public class UpdateProfileValidator : AbstractValidator<UpdateProfileDto>
{
    public UpdateProfileValidator()
    {
        RuleFor(x => x).Must(HasAtLeastOneField).WithMessage("At least one field must be provided");

        When(
            x => x.Bio is not null,
            () =>
            {
                RuleFor(x => x.Bio).NotEmpty().WithMessage("Bio cannot be empty");
            }
        );

        //  When(x => x.xxx is not null, () =>
        // {
        //     RuleFor(x => x.xxx)
        //         .NotEmpty()
        //         .WithMessage("Name cannot be empty");
        // });

        // When(x => x.yyy is not null, () =>
        // {
        //     RuleFor(x => x.yyy)
        //         .LessThan(DateOnly.FromDateTime(DateTime.UtcNow))
        //         .WithMessage("Date of birth must be in the past");
        // });
    }

    private static bool HasAtLeastOneField(UpdateProfileDto dto)
    {
        return dto.Bio is not null;
        // || dto.xxx is not null
        // || dto.yyy is not null;
    }
}
