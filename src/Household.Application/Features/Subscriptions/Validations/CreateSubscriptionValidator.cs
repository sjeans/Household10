using FluentValidation;
using Household.Application.Features.Subscriptions.Commands;

namespace Household.Application.Features.Subscriptions.Validations;

public class CreateSubscriptionValidator : AbstractValidator<CreateSubscription>
{
    public CreateSubscriptionValidator()
    {
        RuleFor(v => v.NewSubscription.Id)
            .LessThanOrEqualTo(0);

        RuleFor(v => v.NewSubscription.Name)
            .MaximumLength(50)
            .NotEmpty()
            .WithMessage("Name is required field!");

        RuleFor(v => v.NewSubscription.PaySchedule)
            .NotEmpty()
            .GreaterThan(subscription => subscription.NewSubscription.StartDate)
            .WithMessage("Need to select a date any it must be today or greater!");
    }
}
