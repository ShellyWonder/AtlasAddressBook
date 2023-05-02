using AtlasAddressBook.Data;
using AtlasAddressBook.Models;
using AtlasAddressBook.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AtlasAddressBook.Services.Repositories
{
    public class ContactRepository : IContactRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly UserRepository _userRepository;

        public ContactRepository(ApplicationDbContext context,
                              UserManager<AppUser> userManager,
                              UserRepository userRepository)
        {
            _context = context;
            _userManager = userManager;
            _userRepository = userRepository;
        }

        public async Task<List<Contact>> GetAllContacts(int userId)
        {
            //explicit syntax
            List<Contact>? contacts = new List<Contact>();
            AppUser appUser = await _userRepository.GetAppUserByIdAsync(userId);
            string appUserId = appUser.Id;

            //return userId & associated contacts & categories}

            appUser = _context.Users
                                .Include(c => c.Contacts)
                                .ThenInclude(c => c.Categories)
                                .FirstOrDefault(u => u.Id == appUserId)!;

            contacts = appUser.Contacts.ToList();

            //return all contacts & categories
            return contacts;
        }
        public async Task<List<Contact>> ContactBySearch(int userId, string searchString)
        {
            List<Contact> contacts = new List<Contact>();
            AppUser appUser = await _userRepository.GetAppUserByIdAsync(userId);
            string appUserId = appUser.Id;
            appUser = _context.Users
                                .Include(c => c.Contacts)
                                .ThenInclude(c => c.Categories)
                                .FirstOrDefault(u => u.Id == appUserId)!;

            if (String.IsNullOrEmpty(searchString))
            {
                contacts = appUser.Contacts
                                   .OrderBy(c => c.LastName)
                                   .ThenBy(c => c.FirstName)
                                   .ToList();
            }
            else
            {
                contacts = appUser.Contacts.Where(c => c.FullName!.ToLower().Contains(searchString.ToLower()))
                                   .OrderBy(c => c.LastName)
                                   .ThenBy(c => c.FirstName)
                                   .ToList();
            }


            //return all contacts & categories
            return contacts;
        }
        public async Task<List<Contact>> AllContactsSearchOption(int userId)
        {
            List<Contact> contacts = new List<Contact>();
            AppUser appUser = await _userRepository.GetAppUserByIdAsync(userId);

            contacts = appUser.Contacts.OrderBy(c => c.LastName)
                                            .ThenBy(c => c.FirstName)
                                            .ToList();
            return contacts;
        }
        public async Task<List<Contact>> SearchResults(int userId, int contactId, int categoryId)
        {
            List<Contact> contacts = new List<Contact>();
            AppUser appUser = await _userRepository.GetAppUserByIdAsync(userId);


            contacts = appUser.Categories.FirstOrDefault(c => c.Id == categoryId)!
                                  .Contacts.OrderBy(c => c.LastName)
                                  .ThenBy(c => c.FirstName)
                                        .ToList();

            return contacts;
        }

        public async Task<Contact> GetEmailContact(int id, int userId, string appUserId)
        {
            AppUser appUser = await _userRepository.GetAppUserByIdAsync(userId);

            Contact? contact = await _context.Contacts!
                                           .Where(c => c.Id == id && c.UserId == appUserId)
                                           .FirstOrDefaultAsync();
            return contact!;
        }
    }
}

