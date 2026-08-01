using Domain.Models;

namespace Application.Common.DTO;

public record UserDTO
{
    public required Guid Id { get; init; }
    public required string UserName { get; init; }
    public Role Role { get; init; }
}

public record UserLoginDTO
{
    public required string UserName { get; init; }
    public required string Password { get; init; }
}

public record UserRegisterDTO
{
    public required string UserName { get; init; }
    public required string Password { get; init; }
    public required Role Role { get; init; }
}