using System;

namespace AuthApp.Features.Auth.DTOs;

public class LoginResponseDto
{

    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }

}
