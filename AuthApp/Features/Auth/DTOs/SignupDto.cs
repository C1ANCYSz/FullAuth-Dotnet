using System;

namespace AuthApp.Features.Auth.DTOs;

public sealed record SignupDto(string Email, string Password, string ConfirmPassword);
