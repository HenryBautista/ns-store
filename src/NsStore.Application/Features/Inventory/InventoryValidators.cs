using FluentValidation;

namespace NsStore.Application.Features.Inventory;

public class StockAdjustmentRequestValidator : AbstractValidator<StockAdjustmentRequest>
{
    public StockAdjustmentRequestValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.QuantityDelta).NotEqual(0);
        RuleFor(x => x.Notes).MaximumLength(400);
    }
}
