using FluentValidation;
using Household.Application.Features.ContactNumbers.Commands;

namespace Household.Application.Features.ContactNumbers.Validations;

public class UpdateContactNumberValidator : AbstractValidator<UpdateContactNumber>
{
    public UpdateContactNumberValidator()
    {
        RuleFor(v => v.UpdatedContactNumber.Id)
            .GreaterThan(0);

        RuleFor(v => v.UpdatedContactNumber.Name)
            .MaximumLength(50)
            .NotEmpty();

        RuleFor(v => v.UpdatedContactNumber.PhoneNumber)
            .MaximumLength(11)
            .NotEmpty();
    }
}
