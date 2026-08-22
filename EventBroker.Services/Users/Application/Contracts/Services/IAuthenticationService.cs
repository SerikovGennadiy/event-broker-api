using Application.Common.DTO;
using Domain.Models;

namespace Application.Contracts.Services.Auth;

public interface IAuthenticationService
{
    Task<(bool IsSuccess, string Token)> RegisterUser(UserRegisterDTO userForRegiatrationDto);
    Task<(bool IsSuccess, string Token)> ValidateUser(UserLoginDTO userForAuthenticationDTO);
}
