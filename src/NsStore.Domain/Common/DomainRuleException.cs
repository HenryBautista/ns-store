namespace NsStore.Domain.Common;

/// <summary>
/// Raised when a domain invariant is violated. Carries a stable, locale-agnostic error code
/// that the SPA maps to a Spanish message.
/// </summary>
public class DomainRuleException(string errorCode, string message) : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
}
