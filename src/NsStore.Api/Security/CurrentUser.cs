using System.Security.Claims;
using NsStore.Application.Common;
using NsStore.Application.Common.Interfaces;
using NsStore.Domain.Common;
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

    public long? HomeBranchId =>
        long.TryParse(Principal?.FindFirstValue(AppClaimTypes.Branch), out var id) ? id : null;

    /// <summary>
    /// Strict override: a header from a non-admin, or one that does not parse, is a 403 rather than
    /// a silent fall back to the home branch. For money and stock a loud failure is the right one —
    /// the alternative lets a client bug write into the wrong branch with no signal.
    /// </summary>
    public long? ActiveBranchId
    {
        get
        {
            var home = HomeBranchId;
            var header = httpContextAccessor.HttpContext?.Request.Headers[AppClaimTypes.BranchHeader].ToString();
            if (string.IsNullOrWhiteSpace(header))
            {
                return home;
            }

            if (!long.TryParse(header, out var requested))
            {
                throw new ForbiddenException(
                    $"{AppClaimTypes.BranchHeader} is not a valid branch id",
                    ErrorCodes.BranchNotAllowed);
            }

            if (!IsAdmin && requested != home)
            {
                throw new ForbiddenException(
                    $"Not allowed to operate in branch {requested}",
                    ErrorCodes.BranchNotAllowed);
            }

            return requested;
        }
    }
}
