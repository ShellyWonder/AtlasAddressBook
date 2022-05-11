
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AtlasAddressBook.Data;
using AtlasAddressBook.Models;
using Microsoft.AspNetCore.Identity;
using AtlasAddressBook.Services.Interfaces;
using AtlasAddressBook.Services;
using AtlasAddressBook.Enums;
using Microsoft.AspNetCore.Authorization;

namespace AtlasAddressBook.Controllers
{
    [Authorize]
    public class ContactsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;
        private readonly UserManager<AppUser> _userManager;
        private readonly ICategoryService _categoryService;
        private readonly IContactService _contactService;
        private readonly SearchService _searchService;
        private readonly DataService _dataService;


        public ContactsController(
            ApplicationDbContext context,
            UserManager<AppUser> userManager,
            ICategoryService categoryService,
            IImageService imageService,
            IContactService contactService,
            SearchService searchService,
            DataService dataService)
        {
            _context = context;
            _userManager = userManager;
            _categoryService = categoryService;
             _imageService = imageService;
            _contactService = contactService;
            _searchService = searchService;
            _dataService = dataService;
        }

        // GET: Contacts
        public async Task<IActionResult> Index()
        {
            string userID = _userManager.GetUserId(User);
            var DBResults = _context.Contacts
                                    .Include(c => c.User)
                                    .Include(c => c.Categories)
                                    .Where(c => c.UserId == userID);
            List<Contact> contacts = await DBResults.ToListAsync();
            return View(contacts);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SearchContacts(string searchString)
        {
            var userId = _userManager.GetUserId(User);

            var model = _searchService.SearchContacts(searchString, userId);
            return View(nameof(Index), model);
        }
        // GET: Contacts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Contact contact = await _contactService.GetContactByIdAsync(id.Value);

            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }
        public async Task<IActionResult> Create()
        {
            string userID = _userManager.GetUserId(User);

            //Edit code For States Enum
            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());
            ViewData["CategoryList"] = new MultiSelectList(await _categoryService.GetUserCategoriesAsync(userID), "Id", "Name");
            return View();
        }
        // POST: Contacts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,Birthday,Address1,Address2,City,State,ZipCode,EmailAddress,PhoneNumber,ImageFile")] Contact contact, List<int> categoriesList)
        {
            if (ModelState.IsValid)
            {
                contact.UserId = _userManager.GetUserId(User);
                contact.Created = _dataService.GetPostGresDate(DateTime.Now);
                if (contact.Birthday != null)
                {
                    contact.Birthday = _dataService.GetPostGresDate(contact.Birthday.Value);

                }
                if (contact.ImageFile != null)
                {

                    contact.ImageData = await _imageService.ConvertFileToByteArrayAsync(contact.ImageFile);
                    contact.ImageType = contact.ImageFile.ContentType;
                }
                _context.Add(contact);
                await _context.SaveChangesAsync();

                await _categoryService.AddContactToCategoriesAsync(categoriesList, contact.Id);
                return RedirectToAction(nameof(Index));
            }
            string userId = _userManager.GetUserId(User);
            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());
            ViewData["CategoryList"] = new MultiSelectList(await _categoryService.GetUserCategoriesAsync(userId), "Id", "Name");
            return View();
        }
        // GET: Contacts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts.FindAsync(id);
            if (contact == null)
            {
                return NotFound();
            }
            string userId = _userManager.GetUserId(User);
            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());
            ViewData["CategoryList"] = new MultiSelectList(await _categoryService.GetUserCategoriesAsync(userId), "Id", "Name", await _categoryService.GetContactCategoryIdsAsync(contact.Id));
            return View(contact);

        }

        // POST: Contacts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,FirstName,LastName,Birthday,Address1,Address2,City,State,ZipCode,Email,PhoneNumber,Created,ImageData,ImageFile")] Contact contact, List<int> categoryList)
        {
            if (id != contact.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    contact.Created = _dataService.GetPostGresDate(contact.Created);
                    if (contact.Birthday is not null)
                    {
                        contact.Birthday = _dataService.GetPostGresDate(contact.Birthday.Value);
                    }
                    if (contact.ImageFile is not null)
                    {
                        contact.ImageData = await _imageService.ConvertFileToByteArrayAsync(contact.ImageFile);
                        contact.ImageType = contact.ImageFile.ContentType;
                    }

                    _context.Update(contact);
                    await _context.SaveChangesAsync();

                    var oldCategories = await _categoryService.GetContactCategoriesAsync(contact.Id);
                    foreach (var category in oldCategories)
                    {
                        await _categoryService.RemoveContactFromCategoryAsync(category.Id, contact.Id);
                    }


                    await _categoryService.AddContactToCategoriesAsync(categoryList, contact.Id);


                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContactExists(contact.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }



            string userId = _userManager.GetUserId(User);
            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());
            ViewData["CategoryList"] = new MultiSelectList(await _categoryService.GetUserCategoriesAsync(userId), "Id", "Name");
            return View(contact);
        }

        // GET: Contacts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // POST: Contacts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            _context.Contacts.Remove(contact);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContactExists(int id)
        {
            return _context.Contacts.Any(e => e.Id == id);
        }
    }

}
