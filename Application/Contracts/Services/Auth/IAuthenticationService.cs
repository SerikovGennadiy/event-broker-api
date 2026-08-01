using Application.Common.DTO;

namespace Application.Contracts.Services.Auth;

internal interface IAuthenticationService
{
    Task<bool> RegisterUser(UserRegisterDTO userForRegiatrationDto);
    Task<bool> ValidateUser(UserLoginDTO userForAuthenticationDTO);
    Task<string> CreateToken();
}
