using AuthApp.Common.Auth;
using AuthApp.Common.Errors;
using AuthApp.Common.Extensions;
using AuthApp.Common.Utils.Security;
using AuthApp.Config;
using AuthApp.Features.Auth.DTOs;
using AuthApp.Features.Jwt;
using AuthApp.Features.User;
using AuthApp.Infrastructure.Auth.Providers;
using AuthApp.Infrastructure.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AuthApp.Features.Auth;

public class AuthService(
    UserRepository userRepository,
    JwtService jwtService,
    AuthRepository authRepository,
    IEmailSender emailSender,
    IOptions<Env> env,
    IEnumerable<IOAuthProvider> providers
)
{
    // NOTE:
    // Password hashing is enforced at DB level via interceptor.
    // Do NOT hash passwords manually in services.
    private readonly UserRepository _userRepository = userRepository;
    private readonly JwtService _jwtService = jwtService;
    private readonly AuthRepository _authRepository = authRepository;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly Env _env = env.Value;

    private readonly Dictionary<AuthProvider, IOAuthProvider> _providers = providers.ToDictionary(
        p => p.Provider
    );

    private async Task SendEmail(string email, string subject, string html)
    {
        await _emailSender.SendAsync(email, subject, html);
    }

    public async Task<TokenPair> Login(LoginDto data)
    {
        var user =
            await _userRepository.FindUserByEmail(data.Email)
            ?? throw new BadRequestException("Invalid Credentials");

        if (user.Provider != AuthProvider.CREDENTIALS)
            throw new BadRequestException(
                "This account uses a different sign-in method. Please use the original one."
            );

        if (user.Password is null)
            throw new InvalidOperationException("Password missing for user");

        if (!PasswordUtil.Verify(data.Password, user.Password))
            throw new BadRequestException("Invalid Credentials");

        var accessToken = _jwtService.GenerateAccessToken(user.Id, user.IsOnboard, user.IsVerified);
        if (!user.IsVerified)
        {
            return new TokenPair(accessToken, null);
        }

        var refreshToken = _jwtService.GenerateRefreshToken(user.Id, user.TokenVersion);

        return new TokenPair(accessToken, refreshToken);
    }

    public async Task<TokenPair> Signup(SignupDto data)
    {
        if (!PasswordUtil.PasswordsAreEqual(data.Password, data.ConfirmPassword))
            throw new BadRequestException("Passwords do not match");

        var exists = await _userRepository.FindUserByEmail(data.Email);
        if (exists is not null)
            throw new BadRequestException("Email already in use");

        var user = await _userRepository.CreateUser(data.Email, data.Password);

        var code = await _authRepository.CreateEmailVerificationCode(user.Id);

        var accessToken = _jwtService.GenerateAccessToken(user.Id, false, false);

        await SendEmail(user.Email, "Verify your email", EmailTemplates.VerifyEmail(code));
        return new TokenPair(accessToken, null);
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
        if (!user.IsVerified)
            throw new UnauthorizedException("Please verify your email");

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

        if (user.Provider != AuthProvider.CREDENTIALS)
            throw new BadRequestException(
                "This account uses a different sign-in method. Please use the original one."
            );

        var rawToken = _authRepository.GenerateResetToken();
        await _authRepository.StoreResetToken(user.Id, rawToken);

        var resetLink =
            $"{_env.BASE_URL}/auth/reset-password?token={Uri.EscapeDataString(rawToken)}";

        await SendEmail(user.Email, "Reset your password", EmailTemplates.ResetPassword(resetLink));
    }

    public async Task ResetPassword(string token, ResetPasswordDto data)
    {
        var userId =
            await _authRepository.ConsumeResetToken(token)
            ?? throw new BadRequestException("Invalid or expired token");

        var user =
            await _userRepository.FindUserById(userId)
            ?? throw new BadRequestException("User not found");

        if (user.Provider != AuthProvider.CREDENTIALS)
            throw new BadRequestException(
                "This account uses a different sign-in method. Please use the original one."
            );

        user.Password = data.Password;
        await _userRepository.SaveAsync();
    }

    public async Task<TokenPair> VerifyEmail(Guid userId, string code)
    {
        var valid = await _authRepository.VerifyEmailCode(userId, code);
        if (!valid)
            throw new BadRequestException("Invalid or expired code");

        var user =
            await _userRepository.FindUserById(userId)
            ?? throw new BadRequestException("User not found");
        if (user.IsVerified)
            throw new BadRequestException("Email already verified");

        user.IsVerified = true;
        await _userRepository.SaveAsync();

        var accessToken = _jwtService.GenerateAccessToken(user.Id, user.IsOnboard, true);
        var refreshToken = _jwtService.GenerateRefreshToken(user.Id, user.TokenVersion);

        return new TokenPair(accessToken, refreshToken);
    }

    public async Task ResendVerificationEmail(Guid userId)
    {
        var user =
            await _userRepository.FindUserById(userId)
            ?? throw new BadRequestException("User not found");

        if (user.IsVerified)
            throw new BadRequestException("Email already verified");

        var allowed = await _authRepository.TrySetEmailCooldown(user.Email);
        if (!allowed)
            return;

        var code = await _authRepository.CreateEmailVerificationCode(user.Id);

        await _emailSender.SendAsync(
            user.Email,
            "Verify your email",
            EmailTemplates.VerifyEmail(code)
        );
    }

    public async Task<TokenPair> OAuthLogin(AuthProvider provider, string token)
    {
        if (provider == AuthProvider.CREDENTIALS)
            throw new ArgumentException("OAuth login does not support credentials provider");

        if (!_providers.TryGetValue(provider, out var oauth))
            throw new InvalidOperationException($"Unsupported OAuth provider: {provider}");

        var identity = await oauth.ValidateAndGetEmailAsync(token);

        var user = await _userRepository.FindUserByEmail(identity.Email);

        if (user is null)
        {
            user = await _userRepository.CreateOAuthUser(
                identity.Email,
                provider,
                identity.ProviderUserId
            );
        }
        else if (user.Provider != provider)
        {
            throw new ConflictException($"User already exists with {user.Provider} provider");
        }

        var accessToken = _jwtService.GenerateAccessToken(
            user.Id,
            isOnboard: user.IsOnboard,
            isVerified: true
        );

        var refreshToken = _jwtService.GenerateRefreshToken(user.Id, user.TokenVersion);

        return new TokenPair(accessToken, refreshToken);
    }
}
