using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NsStore.Application.Common;
using NsStore.Domain.Common;

namespace NsStore.Api.Middleware;

/// <summary>
/// Turns every failure into RFC 7807 ProblemDetails carrying a stable <c>errorCode</c>.
/// The SPA maps the code to Spanish; the API never emits display copy.
/// </summary>
public class AppExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<AppExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, errorCode, detail) = Map(exception);

        if (status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception on {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
        }
        else
        {
            logger.LogInformation(
                "Request failed with {Status} {ErrorCode} on {Method} {Path}",
                status, errorCode, httpContext.Request.Method, httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = status;

        var problemDetails = new ProblemDetails
        {
            Type = $"https://nsstore/errors/{errorCode.ToLowerInvariant().Replace('_', '-')}",
            Title = ReasonPhrase(status),
            Status = status,
            Detail = detail
        };

        problemDetails.Extensions["errorCode"] = errorCode;
        if (exception is ValidationFailedException validation)
        {
            problemDetails.Extensions["errors"] = validation.Errors;
        }

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails
        });
    }

    private static (int Status, string ErrorCode, string Detail) Map(Exception exception) => exception switch
    {
        AppException app => (app.StatusCode, app.ErrorCode, app.Message),
        DomainRuleException domain => (DomainStatus(domain.ErrorCode), domain.ErrorCode, domain.Message),
        DbUpdateConcurrencyException => (
            StatusCodes.Status409Conflict,
            ErrorCodes.ConcurrencyConflict,
            "The record was modified by another operation; retry"),
        _ => (StatusCodes.Status500InternalServerError, ErrorCodes.InternalError, "An unexpected error occurred")
    };

    /// <summary>
    /// Only reached by <see cref="DomainRuleException"/> — an <see cref="AppException"/> already
    /// carries its own status. So of the serial codes only SERIAL_NOT_AVAILABLE belongs here, being
    /// the one an entity raises; listing the rest would be dead code.
    /// </summary>
    private static int DomainStatus(string errorCode) => errorCode switch
    {
        ErrorCodes.InsufficientStock or
        ErrorCodes.PaymentExceedsBalance or
        ErrorCodes.AdvanceExceedsPrice or
        ErrorCodes.SameBranchTransfer or
        ErrorCodes.SerialNotAvailable or
        ErrorCodes.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status400BadRequest
    };

    private static string ReasonPhrase(int status) => status switch
    {
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        409 => "Conflict",
        _ => "Server Error"
    };
}
