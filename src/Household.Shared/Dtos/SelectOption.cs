namespace Household.Shared.Dtos;

// SelectOption class for dropdown options
public class SelectOption
{
    public string Value { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public bool Selected { get; set; } // Indicates whether the option is selected
}
