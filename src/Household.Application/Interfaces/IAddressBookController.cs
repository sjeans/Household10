using Household.Shared.Dtos;

namespace Household.Application.Interfaces;

public interface IAddressBookController
{
    Task<string> CreateShow(AddressInfoDto request);
    Task<List<AddressInfoDto>> GetAddressByFirstLetter(string letter);
    Task<AddressInfoDto> GetAddressById(int id);
    Task<List<AddressInfoDto>> GetAllAddresses();
    Task<string> PutAddressBook(AddressInfoDto updatedAddress);
}