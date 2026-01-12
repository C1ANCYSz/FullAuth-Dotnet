using AuthApp.Common;
using AuthApp.Common.Auth;
using AuthApp.Common.Extensions;
using AuthApp.Features.Jwt;
using AuthApp.Features.User.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthApp.Features.User
{
    [Authorize(Policy = AuthPolicies.Onboard)]
    [Route("api/user")]
    [ApiController]
    public class UserController(UserService userService) : ControllerBase
    {
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.GetUserId();
            return Ok(await userService.GetProfile(userId));
        }

        [Authorize(Policy = AuthPolicies.NotOnboard)]
        public async Task<IActionResult> Onboard(OnboardDto data)
        {
            var userId = User.GetUserId();
            return Ok(await userService.Onboard(userId, data));
        }

        public async Task<IActionResult> UpdateProfle(OnboardDto data)
        {
            var userId = User.GetUserId();
            return Ok(await userService.Onboard(userId, data));
        }
    }
}
