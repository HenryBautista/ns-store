using FluentValidation;

namespace NsStore.Application.Features.Sales;

public class CreateSaleRequestValidator : AbstractValidator<CreateSaleRequest>
{
    public CreateSaleRequestValidator()
    {
        RuleFor(x => x.ClientId).GreaterThan(0);
        RuleFor(x => x.InvoiceType).IsInEnum();
        RuleFor(x => x.PaymentStatus).IsInEnum();
        RuleFor(x => x.SaleDate).NotEqual(default(DateOnly));
        RuleFor(x => x.InitialPaid!.Value).GreaterThanOrEqualTo(0).When(x => x.InitialPaid.HasValue);
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).GreaterThan(0);
            item.RuleFor(i => i.Quantity).GreaterThan(0);
        });
    }
}

public class RegisterPaymentRequestValidator : AbstractValidator<RegisterPaymentRequest>
{
    public RegisterPaymentRequestValidator() => RuleFor(x => x.Amount).GreaterThan(0);
}
