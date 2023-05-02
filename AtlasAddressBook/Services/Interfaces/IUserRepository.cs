using AtlasAddressBook.Models;

namespace AtlasAddressBook.Services.Interfaces
{
    public interface IUserRepository
    {
        Task<AppUser> GetAppUserByIdAsync(int userId);
    }
}