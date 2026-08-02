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

        // Only the shape is checked here. That the allocations add up to Amount, and that each sale
        // can absorb its share, needs the rounded amount and the sales themselves — both live in the
        // service.
        When(x => x.Allocations is not null, () =>
        {
            RuleFor(x => x.Allocations!)
                .NotEmpty()
                .Must(allocations => allocations.Select(a => a.SaleId).Distinct().Count() == allocations.Count)
                // Two lines for one sale would each clear the balance check on their own and
                // together blow past it.
                .WithMessage("Each sale may appear at most once in allocations");

            RuleForEach(x => x.Allocations!).ChildRules(allocation =>
            {
                allocation.RuleFor(a => a.SaleId).GreaterThan(0);
                allocation.RuleFor(a => a.Amount).GreaterThan(0);
            });
        });
    }
}
