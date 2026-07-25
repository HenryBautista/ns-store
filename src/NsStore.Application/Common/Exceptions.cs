using NsStore.Domain.Common;

namespace NsStore.Application.Common;

/// <summary>Base for expected application failures, each mapped to an HTTP status by the API.</summary>
public abstract class AppException(string errorCode, string message) : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
    public abstract int StatusCode { get; }
}

public sealed class NotFoundException(string resource, object key)
    : AppException(ErrorCodes.NotFound, $"{resource} {key} was not found")
{
    public override int StatusCode => 404;
}

public sealed class ConflictException(string errorCode, string message)
    : AppException(errorCode, message)
{
    public override int StatusCode => 409;
}

public sealed class ForbiddenException(string message = "Not allowed to perform this operation")
    : AppException(ErrorCodes.Forbidden, message)
{
    public override int StatusCode => 403;
}

public sealed class UnauthorizedException(string errorCode, string message)
    : AppException(errorCode, message)
{
    public override int StatusCode => 401;
}

public sealed class BadRequestException(string message, string errorCode = ErrorCodes.ValidationError)
    : AppException(errorCode, message)
{
    public override int StatusCode => 400;
}

/// <summary>Field-level validation failure produced by FluentValidation.</summary>
public sealed class ValidationFailedException(IDictionary<string, string[]> errors)
    : AppException(ErrorCodes.ValidationError, "One or more validation errors occurred")
{
    public IDictionary<string, string[]> Errors { get; } = errors;
    public override int StatusCode => 400;
}
