using System.ComponentModel.DataAnnotations;

namespace Household.Shared.Helpers;

public class DateGreaterThanAttribute(string comparisonProperty) : ValidationAttribute
{

    // Validate the date comparison
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return new ValidationResult(ErrorMessage = "Date cannot be empty");

        DateTime currentValue = (DateTime)value;

        DateTime comparisonValue = (DateTime)validationContext.ObjectType.GetProperty(comparisonProperty)?.GetValue(validationContext.ObjectInstance)!;

        if (currentValue < comparisonValue)
            return new ValidationResult(ErrorMessage = "Date must be later than todays date");

        return ValidationResult.Success!;
    }
}
