
namespace AuthApp.Features.Auth.DTOs;

public record class LoginDto
{

    public required string Email { get; init; }
    public required string Password { get; init; }


}
