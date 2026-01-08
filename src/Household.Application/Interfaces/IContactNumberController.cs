using Household.Shared.Dtos;

namespace Household.Application.Interfaces;

public interface IContactNumberController
{
    Task<string> CreateContactNumber(ContactNumberDto request);
    Task<List<ContactNumberDto>> GetAllContactNumbers();
    Task<ContactNumberDto> GetContactById(int id);
    Task<string> PutContactNumber(ContactNumberDto updatedContact);
}