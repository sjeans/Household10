using FluentValidation;
using Household.Application.Features.Shows.Commands;

namespace Household.Application.Features.Shows.Validations;

public class CreateShowValidator : AbstractValidator<CreateShow>
{
    public CreateShowValidator()
    {
        RuleFor(v => v.NewShow.Id)
            .LessThanOrEqualTo(0);

        RuleFor(v => v.NewShow.Name)
            .MaximumLength(50)
            .NotEmpty();

        RuleFor(v => v.NewShow.DayOfWeek)
            .IsInEnum();
        //.Must(i => Enum.IsDefined(typeof(FooEnum), i));

        RuleFor(v => v.NewShow.Episodes)
            .GreaterThan(-1)
            .NotEmpty();

        RuleFor(v => v.NewShow.Season)
            .IsInEnum();

        RuleFor(v => v.NewShow.StreamingId)
            .GreaterThan(0)
            .NotEmpty();
    }
}
