using Users.Application.Common.DTO;

namespace Users.Application.Contracts.Services;

public interface IAuthenticationService
{
    Task<(bool IsSuccess, string Token)> RegisterUser(UserRegisterDTO userForRegiatrationDto);
    Task<(bool IsSuccess, string Token)> ValidateUser(UserLoginDTO userForAuthenticationDTO);
}
