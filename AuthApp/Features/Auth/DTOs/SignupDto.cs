using System;

namespace AuthApp.Features.Auth.DTOs;

public record class SignupDto
{

    public required string Email { get; init; }
    public required string Password { get; init; }
    public required string ConfirmPassword { get; init; }

}
