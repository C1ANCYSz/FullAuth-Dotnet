using AuthApp.Common.Extensions;
using AuthApp.Features.Auth.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthApp.Features.Auth
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController(AuthService authService) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto data)
        {
            var response = await authService.Login(data);
            return Ok(response);
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup(SignupDto data)
        {
            var response = await authService.Signup(data);
            return Ok(response);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto data)
        {
            var response = await authService.RefreshTokens(data.RefreshToken);
            return Ok(response);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.GetUserId();
            await authService.Logout(userId);
            return NoContent();
        }

        // [HttpPost("auth/oauth/{provider}")]
        // public async Task<IActionResult> OAuthLogin(OAuthProvider provider, OAuthLoginDto dto)
        // {
        //     var result = await authService.LoginWithOAuth(provider, dto);
        //     return Ok(result);
        // }
    }
}
