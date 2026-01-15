using AuthApp.Common.Errors;
using AuthApp.Features.Jwt;
using AuthApp.Features.User.DTOs;

namespace AuthApp.Features.User;

public class UserService(UserRepository userRepository, JwtService jwtService)
{
    private readonly UserRepository _userRepository = userRepository;

    private readonly JwtService _jwtService = jwtService;

    public async Task<UserDto> GetProfile(Guid id)
    {
        var user =
            await _userRepository.GetUserProfile(id)
            ?? throw new BadRequestException("User not found");

        if (user.IsOnboard == false)
            throw new BadRequestException("Please complete your profile first");

        return new UserDto(user.Id, user.Email, user.Name, user.Dob, user.Bio);
    }

    public async Task<OnboardDtoResponse> Onboard(Guid id, OnboardDto data)
    {
        var user =
            await _userRepository.Onboard(id, data)
            ?? throw new BadRequestException("User not found");
        var AccessToken = _jwtService.GenerateAccessToken(user.Id, user.IsOnboard);
        var newUser = new UserDto(user.Id, user.Email, user.Name, user.Dob, user.Bio);
        var response = new OnboardDtoResponse(newUser, AccessToken);
        return response;
    }

    public async Task<UserDto> UpdateProfile(Guid id, UpdateProfileDto data)
    {
        var user =
            await _userRepository.UpdateUserProfile(id, data)
            ?? throw new BadRequestException("User not found");

        return new UserDto(user.Id, user.Email, user.Name, user.Dob, user.Bio);
    }

    public async Task<bool> DeleteAccount(Guid id)
    {
        return await _userRepository.DeleteAccount(id);
    }
}
