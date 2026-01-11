using System;
using AuthApp.Common.Exceptions;
using AuthApp.Features.User.DTOs;

namespace AuthApp.Features.User;

public class UserService(UserRepository userRepository)
{
    public async Task<UserDto> GetProfile(Guid id)
    {
        var user = await userRepository.GetUserProfile(id)
            ?? throw new BadRequestException("User not found");

        if (user.IsOnboard == false) throw new BadRequestException("Please complete your profile first");

        return new UserDto(user.Id, user.Email, user.Name, user.Dob, user.Bio);
    }

}
