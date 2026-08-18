using Domain.Models;

namespace Application.Common.DTO;

public record UserDTO
{
    public required Guid Id { get; set; }
    public required string UserName { get; set; }
    public Role Role { get; set; }
}

public record UserLoginDTO
{
    public required string UserName { get; set; }
    public required string Password { get; set; }
}

public record UserRegisterDTO
{
    public required string UserName { get; set; }
    public required string Password { get; set; }
    public required Role Role { get; set; } = Role.User;
}