using FluentValidation;
using Household.Application.Features.Addresses.Commands;

namespace Household.Application.Features.Addresses.Validations;

public class CreateAddressBookCommandValidator : AbstractValidator<CreateAddressBookCommand>
{
    public CreateAddressBookCommandValidator()
    {
        RuleFor(v => v.NewAddress.Id)
            .LessThanOrEqualTo(0);

        RuleFor(v => v.NewAddress.Name)
            .MaximumLength(50)
            .NotEmpty();

        RuleFor(v => v.NewAddress.Address)
            .MaximumLength(75)
            .NotEmpty();
    }
}
