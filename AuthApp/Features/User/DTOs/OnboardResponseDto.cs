using System;

namespace AuthApp.Features.User.DTOs;

public sealed record OnboardDtoResponse(UserDto User, string AccessToken);
