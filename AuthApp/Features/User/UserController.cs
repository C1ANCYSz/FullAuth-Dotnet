using AuthApp.Features.Jwt;
using AuthApp.Features.User.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthApp.Features.User
{
    [Authorize]
    [Route("api/user")]
    [ApiController]
    public class UserController(UserService userService, JwtService jwtService) : ControllerBase
    {

        [HttpGet("profile")]
        public async Task<UserDto> GetProfile()
        {
            var userId = jwtService.GetUserId(User);
            return await userService.GetProfile(userId);
        }
    }
}
