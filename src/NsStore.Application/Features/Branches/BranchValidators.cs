using FluentValidation;

namespace NsStore.Application.Features.Branches;

public class BranchRequestValidator : AbstractValidator<BranchRequest>
{
    public BranchRequestValidator()
    {
        // The code is the document-number prefix, so it stays short and free of separators.
        RuleFor(x => x.Code).NotEmpty().MaximumLength(8).Matches("^[A-Za-z0-9]+$");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Address).MaximumLength(200);
        RuleFor(x => x.Phone).MaximumLength(40);
    }
}
