using System;
using AuthApp.Features.User;

namespace AuthApp.Features.Auth;

public class AuthService(
    UserRepository userRepository,
    AuthRepository authRepository
   )
{




    // public async Task<UserDto?> Login(LoginDto data) => await authRepository.Login(data);

}
