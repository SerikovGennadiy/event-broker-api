using Application.Common.DTO;
using Application.Contracts.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[ApiController]
[Route("auth")]
public class AuthenticationController(IAuthenticationService service) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] UserRegisterDTO userForRegistrationDTO)
    {
        var (isSuccess, _) = await service.RegisterUser(userForRegistrationDTO);

        if (isSuccess)
            return NoContent();
        else
            return BadRequest("Пользователь с таким именем уже существует");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDTO userForAuth)
    {
        var (isSuccess, token) = await service.ValidateUser(userForAuth);
        
        if(isSuccess)
            return Ok(token);
        else
            return BadRequest("Неверные логин или пароль");
    }
}
