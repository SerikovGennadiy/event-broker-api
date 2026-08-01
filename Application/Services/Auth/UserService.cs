using Application.Contracts.Services.Auth;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Application.Services.Auth;

internal class CurrectUserService(IHttpContextAccessor httpContextAccessor): ICurrentUserService
{
    public Guid UserId
    {
        get
        {
            var id = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(id, out var guid) ? guid : Guid.Empty;
        }
    }

    public string? UserName => httpContextAccessor.HttpContext?.User?.Identity?.Name;

    public Role Role => httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role) is { } claim &&
                            Enum.TryParse<Role>(claim.Value, out var role)
                                ? role
                                : Role.User;

    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
}
