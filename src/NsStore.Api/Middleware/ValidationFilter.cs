using FluentValidation;
using NsStore.Application.Common;

namespace NsStore.Api.Middleware;

/// <summary>
/// Runs the FluentValidation validator registered for <typeparamref name="T"/> against the
/// request body before the handler sees it.
/// </summary>
public class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
        var argument = context.Arguments.OfType<T>().FirstOrDefault();

        if (validator is not null && argument is not null)
        {
            var result = await validator.ValidateAsync(argument, context.HttpContext.RequestAborted);
            if (!result.IsValid)
            {
                throw new ValidationFailedException(result.ToDictionary());
            }
        }

        return await next(context);
    }
}

public static class ValidationFilterExtensions
{
    public static RouteHandlerBuilder WithValidation<T>(this RouteHandlerBuilder builder) where T : class =>
        builder.AddEndpointFilter<ValidationFilter<T>>();
}
