namespace NsStore.Domain.Common;

/// <summary>
/// Stable, locale-agnostic error codes returned in ProblemDetails. The SPA maps them to Spanish.
/// </summary>
public static class ErrorCodes
{
    public const string ValidationError = "VALIDATION_ERROR";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    public const string Forbidden = "FORBIDDEN";
    public const string NotFound = "NOT_FOUND";
    public const string Conflict = "CONFLICT";
    public const string InsufficientStock = "INSUFFICIENT_STOCK";
    public const string ConcurrencyConflict = "CONCURRENCY_CONFLICT";
    public const string DuplicateUsername = "DUPLICATE_USERNAME";
    public const string DuplicateName = "DUPLICATE_NAME";
    public const string DuplicateCi = "DUPLICATE_CI";
    public const string PaymentExceedsBalance = "PAYMENT_EXCEEDS_BALANCE";
    public const string AdvanceExceedsPrice = "ADVANCE_EXCEEDS_PRICE";
    public const string PriceNotSet = "PRICE_NOT_SET";
    public const string NoPurchaseHistory = "NO_PURCHASE_HISTORY";
    public const string InvalidRefreshToken = "INVALID_REFRESH_TOKEN";
    public const string BranchNotAllowed = "BRANCH_NOT_ALLOWED";
    public const string BranchInactive = "BRANCH_INACTIVE";
    public const string DuplicateBranchCode = "DUPLICATE_BRANCH_CODE";
    public const string SameBranchTransfer = "SAME_BRANCH_TRANSFER";
    public const string InternalError = "INTERNAL_ERROR";
}
