﻿#nullable disable
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
        private readonly IImageService _imageServices;
        private readonly UserManager<AppUser> _userManager;
        private readonly ICategoryService _categoryService;
        private readonly IContactService _contactService;
        private readonly SearchService _searchService;


        public ContactsController(
            ApplicationDbContext context,
            UserManager<AppUser> userManager,
            ICategoryService categoryService,
            IImageService imageServices,
            IContactService contactService, 
            SearchService searchService)
        {
            _context = context;
            _userManager = userManager;
            _categoryService = categoryService;
            _imageServices = imageServices;
            _contactService = contactService;
            _searchService = searchService;
        }

        // GET: Contacts
        public async Task<IActionResult> Index()
        {
            string userID = _userManager.GetUserId(User);
            var DBResult = _context.Contacts
                                    .Include(c => c.User)
                                    .Include(c => c.Categories)
                                    .Where(c => c.UserId == userID);
            List<Contact> contacts = await DBResult.ToListAsync();
            return View(contacts);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SearchContacts(string searchString)
        {
            var userId =_userManager.GetUserId(User);

            var model = _searchService.SearchContacts(searchString, userId);
            return View (nameof(Index), model);
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

            //Edit code For States Enum
            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());

            string userID = _userManager.GetUserId(User);
            ViewData["CategoryList"] = new MultiSelectList(await _categoryService.GetUserCategoriesAsync(userID), "Id", "Name");
            return View();
        }
        // POST: Contacts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,Birthday,Address1,Address2,City,State,ZipCode,EmailAddress,PhoneNumber,ImageFile")] Contact contact, List<int> contactList)
        {
            if (ModelState.IsValid)
            {
                contact.UserId = _userManager.GetUserId(User);
                contact.Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                if (contact.Birthday != null)
                {
                    contact.Birthday = DateTime.SpecifyKind((DateTime)contact.Birthday, DateTimeKind.Utc);
                }
                if (contact.ImageFile != null)
                {

                    contact.ImageData = await _imageServices.ConvertFileToByteArrayAsync(contact.ImageFile);
                    contact.ImageType = contact.ImageFile.ContentType;
                }
                _context.Add(contact);
                await _context.SaveChangesAsync();

                await _categoryService.AddContactToCategoriesAsync(contactList, contact.Id);
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
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", contact.UserId);
            return View(contact);
        }

        // POST: Contacts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,FirstName,LastName,Birthday,Address1,Address2,City,State,ZipCode,EmailAddress,PhoneNumber,Created,ImageData,ImageType")] Contact contact)
        {
            if (id != contact.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(contact);
                    await _context.SaveChangesAsync();
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
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", contact.UserId);
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