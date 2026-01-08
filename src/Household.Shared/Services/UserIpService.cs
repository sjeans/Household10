using System.Net;
using Household.Shared.Helpers;
using Household.Shared.Services.Interfaces;

namespace Household.Shared.Services;

public class UserIpService(IHttpContextAccessor httpContextAccessor) : IUserIpService
{
    public string CanShow { get; set; } = default!;
    public string IpAddress { get; set; } = default!;
    public string PermissionSetBy { get; set; } = default!;
    public string DisableButton { get; set; } = default!;
    public string Visible { get; set; } = default!;
    public string UrlReferrer { get; set; } = default!;
    public string LogMessage { get; set; } = default!;
    public bool CanSave { get; set; }

    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public void GetUserIP()
    {
        string strIP = string.Empty;
        HttpContext? httpContext = _httpContextAccessor.HttpContext;
        HttpRequest httpReq = httpContext!.Request;

        UrlReferrer = _httpContextAccessor.HttpContext?.Request.Headers["Referer"]!;
        string? RemoteIpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? string.Empty;

        string conditional = string.Empty;

        if (httpReq.Headers["HTTP_CLIENT_IP"].Count > 0)
        {
            conditional = "HTTP_CLIENT_IP";
            strIP = httpReq.Headers["HTTP_CLIENT_IP"]!;
        }
        else if (httpReq.Headers["HTTP_X_FORWARDED_FOR"].Count > 0)
        {
            conditional = "HTTP_X_FORWARDED_FOR";
            strIP = httpReq.Headers["HTTP_X_FORWARDED_FOR"]!;
        }
        else if (!string.IsNullOrEmpty(httpReq.Headers["X-Forwarded-For"]))
        {
            conditional = "X-Forwarded-For";
            strIP = httpReq.Headers["X-Forwarded-For"]!;
        }
        else if (!string.IsNullOrEmpty(httpReq.HttpContext.Connection.RemoteIpAddress?.ToString()))
        {
            conditional = "httpReq.HttpContext.Connection.RemoteIpAddress";
            string rawIpAddress = httpReq.HttpContext.Connection.RemoteIpAddress.ToString();

            if (rawIpAddress.Contains("::ffff:"))
                strIP = rawIpAddress.Replace("::ffff:", "");
            else
                strIP = rawIpAddress;

        }
        else
        {
            // Your existing code for web scraping
        }

        if (UrlReferrer.IsNullOrWhiteSpace())
            UrlReferrer = httpReq.Headers["HTTP_REFERER"]!;

#pragma warning disable CA1416 // Validate platform compatibility
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        if (strIP.Equals("::1"))
        {
            conditional = "::1";
            strIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where(x => x.AddressFamily.ToString().Equals("InterNetwork", StringComparison.Ordinal)).FirstOrDefault().ToString(); //Dns.GetHostEntry(Dns.GetHostName()).AddressList[2].ToString();
        }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CA1416 // Validate platform compatibility

        CheckLocation(strIP, conditional);
    }

    private void CheckLocation(string ipAddress, string whereConditionMet)
    {
        IpAddress = ipAddress;
        PermissionSetBy = whereConditionMet;

        CanSave = IpAddress != "192.168.0.110" && IpAddress != "192.168.0.113";
        CanShow = CanSave ? "display : none;" : "";
        DisableButton = CanSave ? "pointer-events: none; text-decoration: none;" : "";
        Visible = "visibility: " + (CanSave ? "hidden" : "visible");
        LogMessage = $"IP Address ({IpAddress}) that made the request from {PermissionSetBy}.";
    }
}
