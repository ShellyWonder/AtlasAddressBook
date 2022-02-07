using AtlasAddressBook.Models;

namespace AtlasAddressBook.Services.Interfaces
{
    public interface IContactService
    {
        //define method
        public Task<Contact> GetContactByIdAsync(int contactId);
    }
}
