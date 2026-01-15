using AuthApp.Common.Extensions;
using AuthApp.Common.RateLimit;
using AuthApp.Features.Auth.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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
            return Ok(await _authService.Signup(data));
        }

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
        // [HttpPost("auth/oauth/{provider}")]
        // [EnableRateLimiting(RateLimitPolicies.AuthLogin)]
        // public async Task<IActionResult> OAuthLogin(OAuthProvider provider, OAuthLoginDto dto)
        // {
        //     var result = await authService.LoginWithOAuth(provider, dto);
        //     return Ok(result);
        // }
    }
}
