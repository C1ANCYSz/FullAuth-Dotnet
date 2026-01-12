namespace AuthApp.Features.User.DTOs;

public record class UpdateProfileDto
{
    public string? Bio { get; init; }
}
