using FluentValidation;

namespace NsStore.Application.Features.Settings;

public class UpdateSettingsRequestValidator : AbstractValidator<UpdateSettingsRequest>
{
    public UpdateSettingsRequestValidator()
    {
        RuleFor(x => x.VatRate).InclusiveBetween(0, 100);
        RuleFor(x => x.DefaultMarginPct).InclusiveBetween(0, 1000);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
    }
}
