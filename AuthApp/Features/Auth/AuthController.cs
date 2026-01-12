using AuthApp.Features.Auth.DTOs;
using Microsoft.AspNetCore.Http;
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

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            throw new NotImplementedException();
        }

        [HttpGet("refresh-token")]
        public async Task<IActionResult> RefreshToken(string refreshToken)
        {
            var response = await authService.RefreshTokens(refreshToken);
            return Ok(response);
        }
    }


}
