using System;

namespace AuthApp.Features.Auth.DTOs;

public sealed record TokenPair(
    string AccessToken,
    string RefreshToken
);
