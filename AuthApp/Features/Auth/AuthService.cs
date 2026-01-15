using AuthApp.Common.Auth;
using AuthApp.Common.Errors;
using AuthApp.Common.Extensions;
using AuthApp.Common.Utils.Security;
using AuthApp.Config;
using AuthApp.Features.Auth.DTOs;
using AuthApp.Features.Jwt;
using AuthApp.Features.User;
using AuthApp.Infrastructure.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AuthApp.Features.Auth;

public class AuthService(
    UserRepository userRepository,
    JwtService jwtService,
    AuthRepository authRepository,
    IEmailSender emailSender,
    IOptions<Env> env
)
{
    private readonly UserRepository _userRepository = userRepository;
    private readonly JwtService _jwtService = jwtService;
    private readonly AuthRepository _authRepository = authRepository;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly Env _env = env.Value;

    public async Task<LoginResponseDto> Login(LoginDto data)
    {
        var user =
            await _userRepository.FindUserByEmail(data.Email)
            ?? throw new BadRequestException("Invalid Credentials");

        if (user.Provider != AuthProviders.CREDENTIALS)
            throw new BadRequestException(
                "This account uses a different sign-in method. Please use the original one."
            );

        if (user.Password is null)
            throw new InvalidOperationException("Password missing for user");

        if (!PasswordUtil.Verify(data.Password, user.Password))
            throw new BadRequestException("Invalid Credentials");

        var accessToken = _jwtService.GenerateAccessToken(user.Id, user.IsOnboard, user.IsVerified);
        var refreshToken = _jwtService.GenerateRefreshToken(user.Id, user.TokenVersion);

        return new LoginResponseDto { AccessToken = accessToken, RefreshToken = refreshToken };
    }

    public async Task<LoginResponseDto> Signup(SignupDto data)
    {
        var existingUser = await _userRepository.FindUserByEmail(data.Email);
        if (existingUser is not null)
            throw new BadRequestException("Email already in use");

        if (!PasswordUtil.PasswordsAreEqual(data.Password, data.ConfirmPassword))
            throw new BadRequestException("Password and Confirm Password must be same");

        var user = await _userRepository.CreateUser(data.Email, data.Password);

        var accessToken = _jwtService.GenerateAccessToken(user.Id, user.IsOnboard, user.IsVerified);
        var refreshToken = _jwtService.GenerateRefreshToken(user.Id, user.TokenVersion);

        return new LoginResponseDto { AccessToken = accessToken, RefreshToken = refreshToken };
    }

    public async Task<TokenPair> RefreshTokens(string refreshToken)
    {
        var principal =
            _jwtService.VerifyRefreshToken(refreshToken)
            ?? throw new UnauthorizedException("Invalid token");

        var userId = principal.GetUserId();
        var tokenVersion = principal.GetTokenVersion();

        var user =
            await _userRepository.GetUserForTokenRotation(userId)
            ?? throw new UnauthorizedException("Invalid credentials");

        if (user.TokenVersion != tokenVersion)
            throw new UnauthorizedException("Token has been revoked");

        try
        {
            user.TokenVersion++;
            await _userRepository.SaveAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new UnauthorizedException("Refresh token reused");
        }

        var accessToken = _jwtService.GenerateAccessToken(userId, user.IsOnboard, user.IsVerified);
        var newRefreshToken = _jwtService.GenerateRefreshToken(userId, user.TokenVersion);

        return new TokenPair(accessToken, newRefreshToken);
    }

    public async Task Logout(Guid userId)
    {
        var user =
            await _userRepository.FindUserById(userId)
            ?? throw new BadRequestException("User not found");

        user.TokenVersion++;
        await _userRepository.SaveAsync();
    }

    public async Task ForgotPassword(ForgotPasswordDto data)
    {
        var emailAllowed = await _authRepository.TrySetEmailCooldown(data.Email);
        if (!emailAllowed)
            return;

        var user = await _userRepository.FindUserByEmail(data.Email);
        if (user is null)
            return;

        var rawToken = _authRepository.GenerateResetToken();
        await _authRepository.StoreResetToken(user.Id, rawToken);

        var resetLink =
            $"{_env.BASE_URL}/auth/reset-password?token={Uri.EscapeDataString(rawToken)}";

        var template = EmailTemplates.ResetPassword(resetLink);
        await _emailSender.SendAsync(user.Email, "Reset your password", template);
    }

    public async Task ResetPassword(string token, ResetPasswordDto data)
    {
        var userId =
            await _authRepository.GetUserIdByResetToken(token)
            ?? throw new BadRequestException("Invalid or expired token");

        var user =
            await _userRepository.FindUserById(userId)
            ?? throw new BadRequestException("User not found");

        user.Password = data.Password; // assume hashing handled in setter or repo
        await _userRepository.SaveAsync();

        await _authRepository.DeleteResetToken(_authRepository.HashResetToken(token));
    }
}
