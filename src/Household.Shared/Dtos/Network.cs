namespace Household.Shared.Dtos;

public class Network
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Country Country { get; set; } = default!;
    public string OfficalSite { get; set; } = string.Empty;
}

public class Country
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
}
