using Application.Common.DTO;
using Domain.Models;

namespace Application.Contracts.Services.Auth;

public interface ICurrentUserService
{
    Guid UserId { get; }
    string? UserName { get; }
    Role Role { get; }
    bool IsAuthenticated { get; }
}
