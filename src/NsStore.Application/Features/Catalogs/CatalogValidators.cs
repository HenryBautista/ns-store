using FluentValidation;

namespace NsStore.Application.Features.Catalogs;

public class NameRequestValidator : AbstractValidator<NameRequest>
{
    public NameRequestValidator() => RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
}

public class DescriptionRequestValidator : AbstractValidator<DescriptionRequest>
{
    public DescriptionRequestValidator() => RuleFor(x => x.Description).NotEmpty().MaximumLength(120);
}

public class SupplierRequestValidator : AbstractValidator<SupplierRequest>
{
    public SupplierRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Phone).MaximumLength(40);
        RuleFor(x => x.Email).MaximumLength(120).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}
