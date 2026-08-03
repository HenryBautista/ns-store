using FluentValidation;

namespace NsStore.Application.Features.Inventory;

public class RegisterSerialsRequestValidator : AbstractValidator<RegisterSerialsRequest>
{
    public RegisterSerialsRequestValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.SerialNumbers).NotEmpty();
        RuleForEach(x => x.SerialNumbers).NotEmpty().MaximumLength(80);

        // How many may be registered depends on stock the request cannot see, so the count rule
        // lives in the service and throws SERIAL_STOCK_EXCEEDED (409), not a 400 from here.
    }
}
