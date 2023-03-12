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
using AtlasAddressBook.Helpers;
using Microsoft.AspNetCore.Identity.UI.Services;
using AtlasAddressBook.Models.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Emit;
using NuGet.Protocol.Plugins;
using System;
using Microsoft.CodeAnalysis;

namespace AtlasAddressBook.Controllers
{
    [Authorize]
    public class ContactsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IImageService _imageService;
        private readonly ICategoryService _categoryService;
        private readonly IEmailSender _emailService;


        #region Constructor
        public ContactsController(ApplicationDbContext context,
                                    UserManager<AppUser> userManager,
                                    IImageService imageService,
                                    ICategoryService categoryService,
                                    IEmailSender emailService)
        {
            _context = context;
            _userManager = userManager;
            _imageService = imageService;
            _categoryService = categoryService;
            _emailService = emailService;
        }
        #endregion

        #region Get Contacts AllContacts

        // GET: Contacts
        public IActionResult AllContacts(int categoryId, string? swalMessage = null)
        {
            ViewData["SwalMessage"] = swalMessage;
            //explicit syntax
            List<Contact>? contacts = new List<Contact>();
            string appUserId = _userManager.GetUserId(User);


            //return userId & associated contacts & categories
            AppUser appUser = _context.Users
                                           .Include(c => c.Contacts)
                                           .ThenInclude(c => c.Categories)
                                           .FirstOrDefault(u => u.Id == appUserId)!;
            var categories = appUser.Categories;


            //filters contact results by category
            if (categoryId == 0)//All contacts option from filter drop down
            {

                contacts = appUser.Contacts.OrderBy(c => c.LastName)
                                            .ThenBy(c => c.FirstName)
                                            .ToList();
            }
            else // if anything other than zero
            {
                contacts = appUser.Categories.FirstOrDefault(c => c.Id == categoryId)!
                                  .Contacts.OrderBy(c => c.LastName)
                                  .ThenBy(c => c.FirstName)
                                        .ToList();
            }


            // bind categories to select list
            ViewData["CategoryId"] = new SelectList(categories, "Id", "Name", categoryId);

            return View(contacts);
        }
        #endregion

        #region Get Search Contacts
        public IActionResult SearchContacts(string searchString)
        {
            string appUserId = _userManager.GetUserId(User);
            var contacts = new List<Contact>();
            AppUser appUser = _context.Users
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
            ViewData["CategoryId"] = new SelectList(appUser.Categories, "Id", "Name", 0);

            return View(nameof(AllContacts), contacts);
        }
        #endregion

        #region Get Email Contacts
        //Get Email 
        [HttpGet, ActionName("Email")]
        public async Task<IActionResult> EmailContact(int id)
        {
            string appUserId = _userManager.GetUserId(User);
            Contact? contact = await _context.Contacts!
                                           .Where(c => c.Id == id && c.UserId == appUserId)
                                           .FirstOrDefaultAsync();
            if (contact == null)
            {
                return NotFound();
            }

            //instaniating new instance of EmailData object
            EmailData emailData = new EmailData()
            {
                //properties
                EmailAddress = contact.EmailAddress!,
                FirstName = contact.FirstName,
                LastName = contact.LastName

            };

            //instaniating new instance of EmailData object
            EmailContactViewModel model = new EmailContactViewModel()
            {
                //properties
                Contact = contact,
                EmailData = emailData
            };
            return View(model);
        }


        #endregion

        #region Post Email Contacts
        //Post Email Contacts
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmailContact(EmailContactViewModel ecvm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //Send email--use email service
                    await _emailService.SendEmailAsync(ecvm.EmailData!.EmailAddress, ecvm.EmailData.Subject, ecvm.EmailData.Body);
                    //swal msg pops up in AllContacts-success
                    //route value new {. . .}
                    return RedirectToAction("AllContacts", "Contacts", new { swalMessage = "Success: Email Sent" });
                }
                catch (Exception)
                {
                    //swal msg pops up in AllContacts-success fail
                    return RedirectToAction("AllContacts", "Contacts", new { swalMessage = "Error: Email Failed to Send" });

                    throw;
                }
            }
            return View(ecvm);
        }


        #endregion

        #region Get Contact Details
        // GET: Contacts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Contacts == null)
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
        #endregion

        #region Create Get
        // GET: Contacts/Create

        //Task needed with await
        public async Task<IActionResult> Create()
        {
            string appUserId = _userManager.GetUserId(User);

            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());
            ViewData["CategoryList"] = new MultiSelectList(await _categoryService.GetUserCategoriesAsync(appUserId), "Id", "Name");
            return View();
        }
        #endregion

        #region Create Post
        // POST: Contacts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,Birthday,Address1,Address2,City,State,ZipCode,Email,PhoneNumber,Created,ImageFile, ")] Contact contact, List<int> CategoryList)
        {
            //allows for the required AppUserId to be removed from the bind
            ModelState.Remove("AppUserId");
            if (ModelState.IsValid)
            {
                //generates required AppUserID & Created
                contact.UserId = _userManager.GetUserId(User);
                contact.Created = DateTime.SpecifyKind(contact.Birthday!.Value, DateTimeKind.Utc);

                if (contact.Birthday != null)
                {
                    contact.Birthday = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                }
                //checks to see if zip code = 9, the zip code does not begin with 5 zeros or ends in 4 zeros
                
                if (contact.ZipCode != null && contact.ZipCode.Length == 9)
                {
  
                        if (contact.ZipCode.StartsWith("00000") || contact.ZipCode.EndsWith("0000"))
                        {
                           //clear zip code field
                           
                            string message = "Zip code must not begin with 5 zeros or end in 4 zeros.";

                            return new ContentResult { Content = message, ContentType = "text/plain" }; ;
                        }
                   
                }

                if (contact.ImageFile != null)
                {
                    contact.ImageData = await _imageService.ConvertFileToByteArrayAsync(contact.ImageFile);
                    contact.ImageType = contact.ImageFile.ContentType;
                }
                _context.Add(contact);
                await _context.SaveChangesAsync();

                //loop over all the selected categories:
                foreach (int categoryId in CategoryList)
                {
                    await _categoryService.AddContactToCategoriesAsync(contact.Id, categoryId);
                }

                return RedirectToAction(nameof(AllContacts));
            }
            return RedirectToAction(nameof(AllContacts));

        }
        #endregion

        #region Get Contacts Edit
        // GET: Contacts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Contacts == null)
            {
                return NotFound();
            }
            string appUserId = _userManager.GetUserId(User);

            var contact = await _context.Contacts.Where(c => c.Id == id && c.UserId == appUserId)
                                                 .FirstOrDefaultAsync();
            if (contact == null)
            {
                return NotFound();
            }
            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());
            ViewData["CategoryList"] = new MultiSelectList(await _categoryService.GetUserCategoriesAsync(appUserId), "Id", "Name", await _categoryService.GetContactCategoryIdsAsync(contact.Id));

            return View(contact);
        }
        #endregion

        #region Post Contacts Edit
        // POST: Contacts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AppUserId,FirstName,LastName,Birthday,Address1,Address2,City,State,ZipCode,Email,PhoneNumber,Created,ImageFile,ImageData,ImageType")] Contact contact, List<int> CategoryList)
        {
            if (id != contact.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {   //converting time imported from views into proper format accepted by Db
                    contact.Created = DateTime.SpecifyKind(contact.Created, DateTimeKind.Utc);

                    if (contact.Birthday != null)
                    {
                        contact.Birthday = DateTime.SpecifyKind(contact.Birthday!.Value, DateTimeKind.Utc);

                    }
                    //Allowing new image upload to save to Db
                    if (contact.ImageFile != null)
                    {
                        contact.ImageData = await _imageService.ConvertFileToByteArrayAsync(contact.ImageFile);
                        contact.ImageType = contact.ImageFile.ContentType;
                    }
                    _context.Update(contact);
                    await _context.SaveChangesAsync();
                    //save categories
                    //remove current categories
                    List<Category> oldCategories = (await _categoryService.GetContactCategoriesAsync(contact.Id)).ToList();
                    foreach (var category in oldCategories)
                    {
                        await _categoryService.RemoveContactFromCategoryAsync(category.Id, contact.Id);
                    }
                    //add selected categories back to db
                    foreach (int categoryId in CategoryList)
                    {
                        await _categoryService.AddContactToCategoriesAsync(categoryId, contact.Id);
                    }
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
                return RedirectToAction(nameof(AllContacts));
            }
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "Id", contact.UserId);
            return View(contact);
        }
        #endregion

        #region Get Delete
        // GET: Contacts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Contacts == null)
            {
                return NotFound();
            }
            //verify authorized user
            string AppUserId = _userManager.GetUserId(User);
            var contact = await _context.Contacts
                         .FirstOrDefaultAsync(c => c.Id == id && c.UserId == AppUserId);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }
        #endregion

        #region Post Delete
        // POST: Contacts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            string appUserId = _userManager.GetUserId(User);
            var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.Id == id && c.UserId == appUserId);
            if (contact != null)
            {
                _context.Contacts.Remove(contact);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(AllContacts));
        }
        #endregion

        #region ContactExists
        private bool ContactExists(int id)
        {
            return _context.Contacts.Any(e => e.Id == id);
        }
        #endregion
    }

}
