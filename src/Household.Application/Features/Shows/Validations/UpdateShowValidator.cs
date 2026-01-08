using FluentValidation;
using Household.Application.Features.Shows.Commands;

namespace Household.Application.Features.Shows.Validations;

public class UpdateShowValidator : AbstractValidator<UpdateShow>
{
    public UpdateShowValidator()
    {
        RuleFor(v => v.UpdatedShow.Id)
            .GreaterThan(0);

        RuleFor(v => v.UpdatedShow.Name)
            .MaximumLength(50)
            .NotEmpty();

        RuleFor(v => v.UpdatedShow.DayOfWeek)
            .IsInEnum();
        //.Must(i => Enum.IsDefined(typeof(FooEnum), i));

        RuleFor(v => v.UpdatedShow.Episodes)
            .GreaterThan(-1)
            .NotEmpty();

        RuleFor(v => v.UpdatedShow.Season)
            .IsInEnum();

        RuleFor(v => v.UpdatedShow.StreamingId)
            .GreaterThan(0)
            .NotEmpty();
    }
}
