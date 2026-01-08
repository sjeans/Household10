using FluentValidation;
using Household.Application.Features.ContactNumbers.Commands;

namespace Household.Application.Features.ContactNumbers.Validations;

public class CreateContactNumberValidator : AbstractValidator<CreateContactNumber>
{
    public CreateContactNumberValidator()
    {
        RuleFor(v => v.NewContactNumber.Id)
            .LessThanOrEqualTo(0);

        RuleFor(v => v.NewContactNumber.Name)
            .MaximumLength(50)
            .NotEmpty();

        RuleFor(v => v.NewContactNumber.PhoneNumber)
            .MaximumLength(11)
            .NotEmpty();
    }
}
