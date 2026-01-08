namespace Household.Shared.Dtos;

// DTO for transferring user IP details.
public class UserIpDto
{
    public string CanShow { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string PermissionSetBy { get; set; } = string.Empty;
    public string DisableButton { get; set; } = string.Empty;
    public string Visible { get; set; } = string.Empty;
    public string UrlReferrer { get; set; } = string.Empty;
    public string LogMessage { get; set; } = string.Empty;
    public bool CanSave { get; set; }
}
