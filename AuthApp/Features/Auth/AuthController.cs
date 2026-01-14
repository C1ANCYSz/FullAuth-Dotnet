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
        [HttpPost("login")]
        [EnableRateLimiting(RateLimitPolicies.AuthLogin)]
        public async Task<IActionResult> Login(LoginDto data)
        {
            var response = await authService.Login(data);
            return Ok(response);
        }

        [HttpPost("signup")]
        [EnableRateLimiting(RateLimitPolicies.AuthSignup)]
        public async Task<IActionResult> Signup(SignupDto data)
        {
            var response = await authService.Signup(data);
            return Ok(response);
        }

        [HttpPost("refresh-token")]
        [EnableRateLimiting(RateLimitPolicies.AuthRefresh)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto data)
        {
            var response = await authService.RefreshTokens(data.RefreshToken);
            return Ok(response);
        }

        [Authorize]
        [HttpPost("logout")]
        [EnableRateLimiting(RateLimitPolicies.AuthLogout)]
        public async Task<IActionResult> Logout()
        {
            var userId = User.GetUserId();
            await authService.Logout(userId);
            return NoContent();
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
