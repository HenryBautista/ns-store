using NsStore.Application.Common.Interfaces;
using NsStore.Domain.Common;

namespace NsStore.Application.Common;

/// <summary>
/// Resolves the branch a use case operates on.
/// </summary>
/// <remarks>
/// Deliberately not a global <c>HasQueryFilter</c>: EF query filters are all-or-nothing per entity,
/// so an <c>IgnoreQueryFilters()</c> needed for a cross-branch stock read would also drop the
/// soft-delete filter and resurrect deleted products. Nor a middleware or endpoint filter as the
/// sole mechanism — those cannot tell a scoped write from an intentional cross-branch read. The
/// guard lives inside the service, the same place <c>OrderService.EnsureCanEdit</c> puts row-level
/// ownership.
/// </remarks>
public static class BranchScope
{
    /// <summary>The branch to read from: the active one, defaulting reads to the caller's own.</summary>
    public static long RequireBranch(this ICurrentUser user) =>
        user.ActiveBranchId
        ?? throw new ForbiddenException("No active branch resolved for the caller", ErrorCodes.BranchNotAllowed);

    /// <summary>
    /// The branch to write to. A non-admin may only ever write to their home branch, whatever the
    /// request asked for.
    /// </summary>
    public static long RequireWritableBranch(this ICurrentUser user, long? requested = null)
    {
        var active = user.RequireBranch();
        var target = requested ?? active;

        if (!user.IsAdmin && target != user.HomeBranchId)
        {
            throw new ForbiddenException(
                $"Not allowed to operate in branch {target}",
                ErrorCodes.BranchNotAllowed);
        }

        return target;
    }
}
