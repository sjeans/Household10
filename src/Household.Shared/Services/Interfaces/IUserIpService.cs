namespace Household.Shared.Services.Interfaces;

public interface IUserIpService
{
    string CanShow { get; set; }
    string IpAddress { get; set; }
    string PermissionSetBy { get; set; }
    string DisableButton { get; set; }
    string Visible { get; set; }
    string UrlReferrer { get; set; }
    string LogMessage { get; set; }
    bool CanSave { get; set; }

    void GetUserIP();
}
