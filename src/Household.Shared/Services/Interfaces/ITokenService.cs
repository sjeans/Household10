namespace Household.Shared.Services.Interfaces;

public interface ITokenService
{
    Task<string> GetTokenAsync();
    //Task<string> GetTokenAsync(string clientId, string? tenantId = null);
}
