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
    /// The branch to read stock from. Defaults to the active branch, but <b>any authenticated
    /// caller may ask for any branch</b> — never a 403. Seeing that a part sits on another store's
    /// shelf is a different class of sensitivity from seeing that store's takings, and it is the
    /// use case the whole feature exists for.
    /// </summary>
    public static long ResolveReadableBranch(this ICurrentUser user, long? requested = null) =>
        requested ?? user.RequireBranch();

    /// <summary>
    /// The branch filter for money: sales, debts, purchases, reports and the dashboard. A non-admin
    /// is pinned to their home branch whatever they asked for; an admin gets what they asked for,
    /// and <c>null</c> means every branch. This is the one part of the asymmetry that is policy
    /// rather than mechanics.
    /// </summary>
    public static long? ResolveScopedBranch(this ICurrentUser user, long? requested = null)
    {
        if (user.IsAdmin)
        {
            return requested;
        }

        return user.HomeBranchId
            ?? throw new ForbiddenException("No home branch resolved for the caller", ErrorCodes.BranchNotAllowed);
    }

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
