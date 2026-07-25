using FluentValidation;

namespace NsStore.Application.Features.Orders;

public class OrderRequestValidator : AbstractValidator<OrderRequest>
{
    public OrderRequestValidator()
    {
        RuleFor(x => x.OrderDate).NotEqual(default(DateOnly));
        RuleFor(x => x.ClientName).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Phone).MaximumLength(40);
        RuleFor(x => x.ProductDescription).NotEmpty().MaximumLength(400);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AdvanceAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Notes).MaximumLength(400);
        RuleFor(x => x.AdvanceAmount)
            .LessThanOrEqualTo(x => x.Price)
            .WithMessage("Advance amount cannot exceed the price");
    }
}
