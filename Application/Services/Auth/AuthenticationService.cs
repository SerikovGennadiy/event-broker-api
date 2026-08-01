using Application.Common.DTO;
using Application.Contracts.Services.Auth;
using Domain.Models;
using Domain.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.Services.Auth;

internal class AuthenticationService : IAuthenticationService
{
    private readonly JwtSettings _jwtSettings;

    private readonly IOptionsSnapshot<JwtSettings> _configuration;

    public AuthenticationService(IOptionsSnapshot<JwtSettings> configuration, User user)
    {
        _configuration = configuration;
        _jwtSettings = configuration.Value;
    }


   // public Task<string> CreateToken();
    public string CreateToken()
    {
        // Создание списка утверждений
        //    var claims = new List<Claim>
        //{
        //    new Claim(ClaimTypes.Name, request.Username)
        //    // Остальные необходимые утверждения
        //};

        //    // Создание ключа и учётных данных для подписи
        //    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SuperLongSecretKey1234567890123456"));
        //    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        //    // Формирование объекта токена
        //    var token = new JwtSecurityToken(
        //        issuer: "MyAuthServer",
        //        audience: "MyApi",
        //        claims: claims,
        //        expires: DateTime.Now.AddMinutes(15),
        //        signingCredentials: creds
        //    );

        //    // Запись в строку и отправка клиенту
        //    string accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        //    return Ok(new { Token = accessToken });
        return string.Empty;
    }

    public Task<bool> RegisterUser(UserRegisterDTO userForRegiatrationDto)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ValidateUser(UserLoginDTO userForAuthenticationDTO)
    {
        throw new NotImplementedException();
    }

    Task<string> IAuthenticationService.CreateToken()
    {
        throw new NotImplementedException();
    }

    private SigningCredentials GetSignInCredentials()
    {
        if (string.IsNullOrEmpty(_jwtSettings.Secret))
            throw new InvalidOperationException("Сервер не авторизован.");

        var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);
        var secret = new SymmetricSecurityKey(key);

        return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
    }
}
