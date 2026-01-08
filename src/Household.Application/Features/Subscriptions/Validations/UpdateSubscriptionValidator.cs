using FluentValidation;
using Household.Application.Features.Subscriptions.Commands;

namespace Household.Application.Features.Subscriptions.Validations;

public class UpdateSubscriptionValidator : AbstractValidator<UpdateSubscription>
{
    public UpdateSubscriptionValidator()
    {
        RuleFor(v => v.UpdatedSubscription.Id)
            .GreaterThan(0);

        RuleFor(v => v.UpdatedSubscription.Name)
            .MaximumLength(50)
            .NotEmpty()
            .WithMessage("Name is required field!");

        RuleFor(v => v.UpdatedSubscription.PaySchedule)
            .NotEmpty()
            .GreaterThan(subscription => subscription.UpdatedSubscription.StartDate)
            .WithMessage("Need to select a date any it must be today or greater!");
    }
}
