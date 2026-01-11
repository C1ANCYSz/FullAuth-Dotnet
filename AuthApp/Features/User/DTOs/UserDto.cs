using System;

namespace AuthApp.Features.User.DTOs;

public sealed record UserDto
(

    Guid Id,

    string Email,

    string? Name,

    DateOnly? Dob,

    string? Bio
);