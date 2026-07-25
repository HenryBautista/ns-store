using FluentValidation;

namespace NsStore.Application.Features.Users;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(60).Matches("^[A-Za-z0-9._-]+$");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(200);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(80);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(80);
        RuleFor(x => x.MotherLastName).MaximumLength(80);
    }
}

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(60).Matches("^[A-Za-z0-9._-]+$");
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(80);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(80);
        RuleFor(x => x.MotherLastName).MaximumLength(80);
        RuleFor(x => x.Password!).MinimumLength(8).MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Password));
    }
}
