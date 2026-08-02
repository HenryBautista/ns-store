using FluentValidation;

namespace NsStore.Application.Features.Sales;

public class CollectDebtRequestValidator : AbstractValidator<CollectDebtRequest>
{
    public CollectDebtRequestValidator()
    {
        RuleFor(x => x.ClientId).GreaterThan(0);

        // The upper bound is the client's own outstanding balance, which only the service can know;
        // it surfaces as PAYMENT_EXCEEDS_BALANCE rather than a validation error.
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
