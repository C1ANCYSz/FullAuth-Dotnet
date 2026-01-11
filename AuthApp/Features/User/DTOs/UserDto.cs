using System;

namespace AuthApp.Features.User.DTOs;

public class UserDto
{
    public required Guid Id { get; set; }
    public required string Email { get; set; }
    public string? Name { get; set; }
    public DateOnly? Dob { get; set; }
    public string? Bio { get; set; }
}
