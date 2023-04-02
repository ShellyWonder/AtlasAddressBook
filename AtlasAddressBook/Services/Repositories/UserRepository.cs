using AtlasAddressBook.Data;
using AtlasAddressBook.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AtlasAddressBook.Services.Repositories
{
    public class UserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        public UserRepository(ApplicationDbContext context,
                              UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<AppUser> GetAppUserByIdAsync(int userId)
        {
            AppUser appUser = await _userManager.FindByIdAsync(userId.ToString());

            return appUser;
        }
    }
}
