using FluentValidation;
using NsStore.Domain.Enums;

namespace NsStore.Application.Features.Clients;

public class ClientRequestValidator : AbstractValidator<ClientRequest>
{
    public ClientRequestValidator()
    {
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(160);
        RuleFor(x => x.LastName).MaximumLength(80);
        RuleFor(x => x.MotherLastName).MaximumLength(80);
        RuleFor(x => x.Ci).MaximumLength(30);
        RuleFor(x => x.Nit).MaximumLength(30);
        RuleFor(x => x.Phone).MaximumLength(40);
        RuleFor(x => x.Email).MaximumLength(120).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.City).MaximumLength(80);
        RuleFor(x => x.Address).MaximumLength(200);
        RuleFor(x => x.ContactName).MaximumLength(120);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .When(x => x.Type == ClientType.Individual)
            .WithMessage("Last name is required for an individual client");
    }
}
