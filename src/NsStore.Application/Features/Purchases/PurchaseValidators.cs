using FluentValidation;

namespace NsStore.Application.Features.Purchases;

public class CreatePurchaseRequestValidator : AbstractValidator<CreatePurchaseRequest>
{
    public CreatePurchaseRequestValidator()
    {
        RuleFor(x => x.SupplierId).GreaterThan(0);
        RuleFor(x => x.InvoiceType).IsInEnum();
        RuleFor(x => x.PaymentStatus).IsInEnum();
        RuleFor(x => x.PurchaseDate).NotEqual(default(DateOnly));
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).GreaterThan(0);
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}
