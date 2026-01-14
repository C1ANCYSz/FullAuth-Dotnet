using AuthApp.Common.Auth.Attributes;
using AuthApp.Common.Extensions;
using AuthApp.Features.User.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthApp.Features.User
{
    // [Authorize(Policy = AuthPolicies.Onboard)]
    [Authorize]
    [RequireOnboard]
    [Route("api/users/me")]
    [ApiController]
    public class UserController(UserService userService) : ControllerBase
    {
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.GetUserId();
            return Ok(await userService.GetProfile(userId));
        }

        [SkipOnboardCheck]
        [HttpPost("onboard")]
        public async Task<IActionResult> Onboard([FromBody] OnboardDto data)
        {
            var userId = User.GetUserId();
            return Ok(await userService.Onboard(userId, data));
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfle(UpdateProfileDto data)
        {
            var userId = User.GetUserId();
            return Ok(await userService.UpdateProfile(userId, data));
        }

        [HttpDelete("delete-account")]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = User.GetUserId();
            await userService.DeleteAccount(userId);
            return NoContent();
        }
    }
}
