using Enums.Users;
namespace Users.Application.Contracts.Services;

public interface ICurrentUserService
{
    Guid UserId { get; }
    string? UserName { get; }
    Role Role { get; }
    bool IsAuthenticated { get; }
}
