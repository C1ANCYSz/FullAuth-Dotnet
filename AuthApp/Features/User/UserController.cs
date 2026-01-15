using AuthApp.Common.Auth.Attributes;
using AuthApp.Common.Extensions;
using AuthApp.Common.RateLimit;
using AuthApp.Features.User.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AuthApp.Features.User
{
    [Authorize]
    [IsVerified]
    [RequireOnboard]
    [Route("api/users/me")]
    [ApiController]
    public class UserController(UserService userService) : ControllerBase
    {
        private readonly UserService _userService = userService;

        [HttpGet("profile")]
        [EnableRateLimiting(RateLimitPolicies.UserRead)]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.GetUserId();
            return Ok(await _userService.GetProfile(userId));
        }

        [SkipOnboardCheck]
        [HttpPost("onboard")]
        [EnableRateLimiting(RateLimitPolicies.UserWrite)]
        public async Task<IActionResult> Onboard([FromBody] OnboardDto data)
        {
            var userId = User.GetUserId();
            return Ok(await _userService.Onboard(userId, data));
        }

        [HttpPut("profile")]
        [EnableRateLimiting(RateLimitPolicies.UserWrite)]
        public async Task<IActionResult> UpdateProfle(UpdateProfileDto data)
        {
            var userId = User.GetUserId();
            return Ok(await _userService.UpdateProfile(userId, data));
        }

        [HttpDelete("delete-account")]
        [EnableRateLimiting(RateLimitPolicies.UserSensitive)]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = User.GetUserId();
            await _userService.DeleteAccount(userId);
            return NoContent();
        }
    }
}
