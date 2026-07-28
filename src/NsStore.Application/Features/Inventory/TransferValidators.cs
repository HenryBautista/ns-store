using FluentValidation;

namespace NsStore.Application.Features.Inventory;

public class CreateTransferRequestValidator : AbstractValidator<CreateTransferRequest>
{
    public CreateTransferRequestValidator()
    {
        RuleFor(x => x.OriginBranchId).GreaterThan(0);
        RuleFor(x => x.DestinationBranchId).GreaterThan(0);
        RuleFor(x => x.Notes).MaximumLength(400);
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).GreaterThan(0);
            item.RuleFor(i => i.Quantity).GreaterThan(0);
        });

        // Origin == destination is a domain rule, not a field rule: it throws
        // SAME_BRANCH_TRANSFER (409) from the service rather than a 400 from here.
    }
}
