using Household.Shared.Models;

namespace Household.Application.Interfaces;

public interface IUserTypeController
{
    Task<List<UserType>> GetAllUserTypesAsync();
    Task<UserType?> GetUserTypeById(int id);
}