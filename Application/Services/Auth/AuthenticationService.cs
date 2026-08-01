using Application.Common.DTO;
using Application.Contracts.Persistance;
using Application.Contracts.Services.Auth;
using Domain.Exceptions;
using Domain.Exceptions.Auth;
using Domain.Models;
using Domain.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.Services.Auth;

public class AuthenticationService(IRepositoryManager repo,
                                   IHashService hashService,
                                   IOptionsSnapshot<JwtSettings> config) : IAuthenticationService
{
    public async Task<(bool IsSuccess, string Token)> RegisterUser(UserRegisterDTO userDTO)
    {
        if (string.IsNullOrEmpty(userDTO.UserName) || string.IsNullOrEmpty(userDTO.Password))
            throw new UserValidationException();

        var user = await repo.User.GetUserByNameAsync(userDTO.UserName);
        if (user is not null)
            throw new WhoAreYouException($"{nameof(AuthenticationService)}: пользователь с именем {userDTO.UserName} уже существет");

        var passwordHash = hashService.Hash(userDTO.Password);
        var newUser = User.Restore(Guid.CreateVersion7(), userDTO.UserName, passwordHash, userDTO.Role);

        repo.User.CreateUser(newUser);
        await repo.SaveAsync();

        return (IsSuccess: true, Token: CreateToken(newUser));
    }

    public async Task<(bool IsSuccess, string Token)> ValidateUser(UserLoginDTO userDTO)
    {
        if (string.IsNullOrEmpty(userDTO.UserName) || string.IsNullOrEmpty(userDTO.Password))
            throw new UserValidationException();

        var user = await repo.User.GetUserByNameAsync(userDTO.UserName);
        if (user is null)
           throw new WhoAreYouException($"{nameof(AuthenticationService)}: неправильные логин или пароль");

        // Проверяем введённый пароль против сохранённого хеша пользователя
        var hashMatches = hashService.Verify(userDTO.Password, user.PasswordHash ?? string.Empty);
        if(!hashMatches)
            throw new WhoAreYouException($"{nameof(AuthenticationService)}: неправильные логин или пароль");

        return (IsSuccess: hashMatches, Token: hashMatches ? CreateToken(user) : string.Empty);
    }


    private string CreateToken(User user)
    {
        var signKey = GetSignKey();
        var claims = GetClaims(user);
        var tokenOptions = GenerateToken(claims, signKey);
        // TODO: добавить запись refresh токена в БД, чтобы можно было его отозвать
        return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
    }
    private SigningCredentials GetSignKey()
    {
        var _jwtSettings = config.Value;

        if (string.IsNullOrEmpty(_jwtSettings.Secret))
            throw new SecurityTokenException ($"{nameof(AuthenticationService)}: на сервере не настроена подсистема доступа");

        var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);
        var secret = new SymmetricSecurityKey(key);

        return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
    }
    private IEnumerable<Claim> GetClaims(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        return claims;
    }
    private JwtSecurityToken GenerateToken(IEnumerable<Claim> claims, SigningCredentials signingCredentials)
    {
        var _jwtSettings = config.Value;

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.ValidIssuer,
            audience: _jwtSettings.ValidAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresMinutes),
            signingCredentials: signingCredentials
        );
        return token;
    }
}
