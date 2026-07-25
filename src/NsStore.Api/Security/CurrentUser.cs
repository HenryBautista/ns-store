using System.Security.Claims;
using NsStore.Application.Common.Interfaces;
using NsStore.Domain.Enums;

namespace NsStore.Api.Security;

/// <summary>Reads the authenticated principal off the current request's JWT claims.</summary>
public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public long? UserId =>
        long.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? Principal?.FindFirstValue("sub"), out var id)
            ? id
            : null;

    public string? Username => Principal?.FindFirstValue(ClaimTypes.Name);

    public UserRole? Role =>
        Enum.TryParse<UserRole>(Principal?.FindFirstValue(ClaimTypes.Role), ignoreCase: true, out var role)
            ? role
            : null;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public bool IsAdmin => Role == UserRole.Admin;
}
