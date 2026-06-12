using Users.DTOs;

namespace Users.Service.Interface;

public interface IUserService
{
    Task<LoginTokenResponseDto?> LoginUserAsync(UserLoginDto userLoginDto);
}