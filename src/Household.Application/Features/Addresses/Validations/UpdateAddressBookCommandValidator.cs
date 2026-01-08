using FluentValidation;
using Household.Application.Features.Addresses.Commands;

namespace Household.Application.Features.Addresses.Validations;

public class UpdateAddressBookCommandValidator : AbstractValidator<UpdateAddressBookCommand>
{
    public UpdateAddressBookCommandValidator()
    {
        RuleFor(v => v.UpdatedAddress.Id)
            .GreaterThan(0);

        RuleFor(v => v.UpdatedAddress.Name)
            .MaximumLength(50)
            .NotEmpty();

        RuleFor(v => v.UpdatedAddress.Address)
            .MaximumLength(75)
            .NotEmpty();

        RuleFor(v => v.UpdatedAddress.City)
            .NotEmpty();

        RuleFor(v => v.UpdatedAddress.State)
            .NotEmpty();

        RuleFor(v => v.UpdatedAddress.CountryCode)
            .NotEmpty();
    }
}
