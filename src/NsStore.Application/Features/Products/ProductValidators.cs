using FluentValidation;

namespace NsStore.Application.Features.Products;

public class ProductRequestValidator : AbstractValidator<ProductRequest>
{
    public ProductRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(160);
        RuleFor(x => x.PartNumber).MaximumLength(80);
        RuleFor(x => x.Description).MaximumLength(400);
        RuleFor(x => x.SerialNumber).MaximumLength(80);
    }
}

public class SetPricesRequestValidator : AbstractValidator<SetPricesRequest>
{
    public SetPricesRequestValidator()
    {
        RuleFor(x => x.PriceWithInvoice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PriceWithoutInvoice).GreaterThanOrEqualTo(0);
    }
}
