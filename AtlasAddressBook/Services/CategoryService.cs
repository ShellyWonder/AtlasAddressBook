using AtlasAddressBook.Data;
using AtlasAddressBook.Models;
using AtlasAddressBook.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AtlasAddressBook.Services
{
    
   
        public class CategoryService : ICategoryService
        {
            private readonly ApplicationDbContext _context;

            public CategoryService(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task AddContactToCategoryAsync(int categoryId, int contactId)
            {
                if (!await IsContactInCategory(categoryId, contactId))
                {

                    Contact contact = (await _context.Contacts.FindAsync(contactId))!;

                    Category category = (await _context.Categories.FindAsync(categoryId))!;
                    if (category is not null && contact != null)
                    {

                        category.Contacts.Add(contact);

                        //Alternate
                        //contact.Categories.Add(category);

                        await _context.SaveChangesAsync();
                    }
                }
            }

            public async Task AddContactToCategoriesAsync(List<int> categoryList, int contactId)
            {

                foreach (var categoryId in categoryList)
                {
                    if (!await IsContactInCategory(categoryId, contactId))
                    {

                        var contact = await _context.Contacts.FindAsync(contactId);

                        var category = await _context.Categories.FindAsync(categoryId);
                        if (category != null && contact != null)
                        {

                            contact.Categories.Add(category);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
            }

            public async Task<ICollection<Category>> GetContactCategoriesAsync(int contactId)
            {
                var contact = await _context.Contacts.Include(c => c.Categories).FirstOrDefaultAsync(c => c.Id == contactId);
                return contact!.Categories;
            }

            public async Task<ICollection<int>> GetContactCategoryIdsAsync(int contactId)
            {
                var categoryIds = (await GetContactCategoriesAsync(contactId)).Select(c => c.Id).ToList();
                return categoryIds;
            }


            public async Task<bool> IsContactInCategory(int categoryId, int contactId)
            {
                Contact contact = (await _context.Contacts.FindAsync(contactId))!;

                return await _context.Categories
                    .Include(c => c.Contacts)
                    .Where(c => c.Id == categoryId && c.Contacts.Contains(contact))
                    .AnyAsync();
            }

            public async Task RemoveContactFromCategoryAsync(int categoryId, int contactId)
            {
                if (await IsContactInCategory(categoryId, contactId))
                {
                    var contact = await _context.Contacts.FindAsync(contactId);

                    var category = await _context.Categories.FindAsync(categoryId);

                    if (category is not null && contact is not null)
                    {
                        category.Contacts.Remove(contact);

                        //Alternate
                        //contact.Categories.Remove(category);

                        await _context.SaveChangesAsync();
                    }

                }
            }

            public async Task<IEnumerable<Category>> GetUserCategoriesAsync(string userId)
            {
                return await _context.Categories.Where(c => c.UserId == userId).ToListAsync();
            }
        }
    
}
