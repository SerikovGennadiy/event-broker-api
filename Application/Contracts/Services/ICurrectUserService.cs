using Application.Common.DTO;
using Domain.Models;

namespace Application.Contracts.Services;

public interface ICurrectUserService
{
    Guid UserId { get; }
    string? UserName { get; }
    Role Role { get; }
    bool IsAuthenticated { get; }
}
