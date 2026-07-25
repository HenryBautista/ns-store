using FluentValidation;

namespace NsStore.Application.Features.Auth;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(60);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(200);
    }
}
