using AtlasAddressBook.Models;

namespace AtlasAddressBook.Services.Interfaces
{
    public interface IContactRepository
    {
        Task<List<Contact>> AllContactsSearchOption(int userId);
        Task<List<Contact>> ContactBySearch(int userId, string searchString);
        Task<List<Contact>> GetAllContacts(int userId);
        Task<Contact> GetEmailContact(int id, int userId, string appUserId);
        Task<List<Contact>> SearchResults(int userId, int contactId, int categoryId);
    }
}