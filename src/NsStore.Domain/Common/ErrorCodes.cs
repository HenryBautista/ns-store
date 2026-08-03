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

    /// <summary>The serial is already registered — on any product, in any status.</summary>
    public const string DuplicateSerialNumber = "DUPLICATE_SERIAL_NUMBER";

    /// <summary>Named serial does not exist, sits in another branch, or has already left stock.</summary>
    public const string SerialNotAvailable = "SERIAL_NOT_AVAILABLE";

    /// <summary>
    /// Fewer serials were picked than the units being moved require. The detail carries the
    /// quantity, the stock on hand, how many units are identified and the resulting bounds —
    /// "pick more" is useless to a seller without the numbers.
    /// </summary>
    public const string SerialSelectionRequired = "SERIAL_SELECTION_REQUIRED";

    /// <summary>More serials than units, or an inbound line whose count does not match its quantity.</summary>
    public const string SerialCountMismatch = "SERIAL_COUNT_MISMATCH";

    /// <summary>Serials were supplied for a product that is not tracked per unit.</summary>
    public const string SerialsNotTracked = "SERIALS_NOT_TRACKED";

    /// <summary>Registering these serials would identify more units than the branch actually holds.</summary>
    public const string SerialStockExceeded = "SERIAL_STOCK_EXCEEDED";

    /// <summary>Per-unit tracking cannot be turned off while identified units are still in stock.</summary>
    public const string SerializationInUse = "SERIALIZATION_IN_USE";

    public const string InternalError = "INTERNAL_ERROR";
}
