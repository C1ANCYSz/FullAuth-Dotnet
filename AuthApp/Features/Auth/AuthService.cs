using AuthApp.Common.Auth;
using AuthApp.Common.Errors;
using AuthApp.Common.Extensions;
using AuthApp.Common.Utils.Security;
using AuthApp.Features.Auth.DTOs;
using AuthApp.Features.Jwt;
using AuthApp.Features.User;
using Microsoft.EntityFrameworkCore;

namespace AuthApp.Features.Auth;

public class AuthService(UserRepository userRepository, JwtService jwtService)
{
    public async Task<LoginResponseDto> Login(LoginDto data)
    {
        var user =
            await userRepository.FindUserByEmail(data.Email)
            ?? throw new BadRequestException("Invalid Credentials");

        if (user.Provider != AuthProviders.CREDENTIALS)
            throw new BadRequestException(
                "This account uses a different sign-in method. Please use the original one."
            );

        if (user.Password is null)
            throw new InvalidOperationException("Password missing for user");

        var password = PasswordUtil.Verify(data.Password, user.Password);

        if (!password)
            throw new BadRequestException("Invalid Credentials");

        var AccessToken = jwtService.GenerateAccessToken(user.Id, user.IsOnboard);
        var RefreshToken = jwtService.GenerateRefreshToken(user.Id, user.TokenVersion);

        return new LoginResponseDto { AccessToken = AccessToken, RefreshToken = RefreshToken };
    }

    public async Task<LoginResponseDto> Signup(SignupDto data)
    {
        var existingUser = await userRepository.FindUserByEmail(data.Email);
        if (existingUser is not null)
            throw new BadRequestException("Email already in use");

        if (!PasswordUtil.PasswordsAreEqual(data.Password, data.ConfirmPassword))
            throw new BadRequestException("Password and Confirm Password must be same");

        var user = await userRepository.CreateUser(data.Email, data.Password);

        var accessToken = jwtService.GenerateAccessToken(user.Id, false);

        var refreshToken = jwtService.GenerateRefreshToken(user.Id, 0);

        return new LoginResponseDto { AccessToken = accessToken, RefreshToken = refreshToken };
    }

    public async Task<TokenPair> RefreshTokens(string refreshToken)
    {
        var principal =
            jwtService.VerifyRefreshToken(refreshToken)
            ?? throw new UnauthorizedException("Invalid token");

        var userId = principal.GetUserId();

        var tokenVersion = principal.GetTokenVersion();

        var user =
            await userRepository.GetUserForTokenRotation(userId)
            ?? throw new UnauthorizedException("Invalid credentials");

        if (user.TokenVersion != tokenVersion)
            throw new UnauthorizedException("Token has been revoked");

        try
        {
            user.TokenVersion++;
            await userRepository.SaveAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new UnauthorizedException("Refresh token reused");
        }

        var accessToken = jwtService.GenerateAccessToken(userId, user.IsOnboard);
        var newRefreshToken = jwtService.GenerateRefreshToken(userId, user.TokenVersion);

        return new TokenPair(accessToken, newRefreshToken);
    }

    public async Task Logout(Guid userId)
    {
        var user =
            await userRepository.FindUserById(userId)
            ?? throw new BadRequestException("User not found");

        user.TokenVersion++;
        await userRepository.SaveAsync();
    }
}
