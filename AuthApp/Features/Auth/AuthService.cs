using System.IdentityModel.Tokens.Jwt;
using AuthApp.Common.Exceptions;
using AuthApp.Common.Utils;
using AuthApp.Features.Auth.DTOs;
using AuthApp.Features.Jwt;
using AuthApp.Features.User;


namespace AuthApp.Features.Auth;

public class AuthService(
    UserRepository userRepository,
    // AuthRepository authRepository,
    JwtService jwtService
   )
{

    public async Task<LoginResponseDto?> Login(LoginDto data)
    {
        var user = await userRepository.FindUserByEmail(data.Email)
         ?? throw new BadRequestException("User not found");

        var password = PasswordUtil.Verify(data.Password, user.Password);

        if (!password) throw new BadRequestException("Invalid Credentials");

        var AccessToken = jwtService.GenerateAccessToken(user.Id);
        var RefreshToken = jwtService.GenerateRefreshToken(user.Id);
        return new LoginResponseDto { AccessToken = AccessToken, RefreshToken = RefreshToken };
    }


    public async Task<LoginResponseDto?> Signup(SignupDto data)
    {
        var existingUser = await userRepository.FindUserByEmail(data.Email);
        if (existingUser is not null)
            throw new BadRequestException("Email already in use");

        if (!PasswordUtil.PasswordsAreEqual(data.Password, data.ConfirmPassword))
            throw new BadRequestException("Password and ConfirmPassword must be same");

        var user = await userRepository.CreateUser(data.Email, data.Password);
        var AccessToken = jwtService.GenerateAccessToken(user.Id);
        var RefreshToken = jwtService.GenerateRefreshToken(user.Id);
        return new LoginResponseDto { AccessToken = AccessToken, RefreshToken = RefreshToken };
    }

    public async Task<TokenPair> RefreshTokens(string refreshToken)
    {
        var principal = jwtService.VerifyRefreshToken(refreshToken)
            ?? throw new BadRequestException("Invalid token");

        var userId = jwtService.GetUserId(principal);

        var accessToken = jwtService.GenerateAccessToken(userId);
        var newRefreshToken = jwtService.GenerateRefreshToken(userId);

        return new TokenPair(accessToken, newRefreshToken);
    }


}
