using AuthApp.Common.Auth;
using AuthApp.Common.Extensions;
using AuthApp.Common.RateLimit;
using AuthApp.Features.Auth.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StackExchange.Redis;

namespace AuthApp.Features.Auth
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController(AuthService authService) : ControllerBase
    {
        private readonly AuthService _authService = authService;

        [HttpPost("login")]
        [EnableRateLimiting(RateLimitPolicies.AuthLogin)]
        public async Task<IActionResult> Login(LoginDto data)
        {
            return Ok(await _authService.Login(data));
        }

        [HttpPost("signup")]
        [EnableRateLimiting(RateLimitPolicies.AuthSignup)]
        public async Task<IActionResult> Signup(SignupDto data)
        {
            await _authService.Signup(data);
            return Ok(new { message = "Account has been created successfully" });
        }

        [Authorize]
        [HttpPost("refresh-token")]
        [EnableRateLimiting(RateLimitPolicies.AuthRefresh)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto data)
        {
            return Ok(await _authService.RefreshTokens(data.RefreshToken));
        }

        [Authorize]
        [HttpPost("logout")]
        [EnableRateLimiting(RateLimitPolicies.AuthLogout)]
        public async Task<IActionResult> Logout()
        {
            var userId = User.GetUserId();
            await _authService.Logout(userId);
            return NoContent();
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [EnableRateLimiting(RateLimitPolicies.AuthForgotPassword)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto data)
        {
            await _authService.ForgotPassword(data);

            return Ok(new { message = "Password reset link has been sent to your email" });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        [EnableRateLimiting(RateLimitPolicies.AuthResetPassword)]
        public async Task<IActionResult> ResetPassword(
            [FromQuery] string token,
            [FromBody] ResetPasswordDto data
        )
        {
            await _authService.ResetPassword(token, data);

            return Ok(new { message = "Password has been reset successfully" });
        }

        [Authorize]
        [HttpPost("verify-email")]
        [EnableRateLimiting(RateLimitPolicies.AuthVerifyEmail)]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto data)
        {
            var userId = User.GetUserId();

            return Ok(await _authService.VerifyEmail(userId, data.Code));
        }

        [Authorize]
        [HttpPost("resend-verification")]
        [EnableRateLimiting(RateLimitPolicies.AuthVerifyEmail)]
        public async Task<IActionResult> ResendVerification()
        {
            var userId = User.GetUserId();
            await _authService.ResendVerificationEmail(userId);

            return Ok(new { message = "Verification email sent" });
        }

        // REDIRECT

        // [HttpGet("auth/oauth/{provider}")]
        // [AllowAnonymous]
        // [EnableRateLimiting(RateLimitPolicies.AuthLogin)]
        // public IActionResult OAuthLogin(AuthProvider provider)
        // {
        //     return provider switch
        //     {
        //         AuthProvider.GOOGLE => Challenge("Google"),
        //         AuthProvider.GITHUB => Challenge("GitHub"),
        //         _ => BadRequest("Provider does not support OAuth"),
        //     };
        // }

        [AllowAnonymous]
        [HttpPost("oauth/{provider}")]
        public async Task<IActionResult> OAuthLogin(AuthProvider provider, [FromBody] OAuthDto dto)
        {
            switch (provider)
            {
                case AuthProvider.GOOGLE:
                {
                    var result = await _authService.OAuthLogin(AuthProvider.GOOGLE, dto.Token);

                    return Ok(result);
                }

                case AuthProvider.GITHUB:
                {
                    var result = await _authService.OAuthLogin(AuthProvider.GITHUB, dto.Token);

                    return Ok(result);
                }

                default:
                    return BadRequest("Unsupported provider");
            }
        }
    }
}
