using AtlasAddressBook.Enums;
using AtlasAddressBook.Models;
using AtlasAddressBook.Services;
using AtlasAddressBook.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AtlasAddressBook.Data
{
    public static class DataUtility
    {
        //Every method in a static class must be static as well
        public static string? GetConnectionString(IConfiguration configuration)
        {
            //Local environment
            var connectionString = configuration.GetSection("ConnectionStrings")["DefaultConnection"];

            //Identifies the deployment environment
            //Environment variable changes according to host
            //may need to change when deploying to RailWay
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");//Heroku Environment variable name

            return string.IsNullOrEmpty(databaseUrl) ? connectionString : BuildConnectionString(databaseUrl);
        }
        //building connection string from the heroku environment
        private static string BuildConnectionString(string databaseUrl)
        {
            //postgres specific
            //converts URL to URI; URL locates a resource, URI identifies a resource
            var databaseUri = new Uri(databaseUrl);
            var userInfo = databaseUri.UserInfo.Split(':');
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,//may need to change when deploying to RailWay
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseUri.LocalPath.TrimStart('/'),
                //encrypts the data
                SslMode = SslMode.Require,
                TrustServerCertificate = true
            };
            return builder.ToString();
        }
        public static async Task ManageDataAsync(IServiceProvider svcProvider)
        {
            //Service: An instance of applicationDbContext
            var dbContextSvc = svcProvider.GetRequiredService<ApplicationDbContext>();
            //Service: An instance of UserManager<AppUser>
            var userManagerSvc = svcProvider.GetRequiredService<UserManager<AppUser>>();
            //Service: An instance of CategoryService
            //CategoryService categorySvc = svcProvider.GetRequiredService<CategoryService>();
            //same functionality as update-database
            await dbContextSvc.Database.MigrateAsync();

            //Custom Address Book seeding methods
            await SeedDefaultUserAsync(userManagerSvc);
            await SeedDefaultContacts(dbContextSvc);
            await SeedDefaultCategoriesAsync(dbContextSvc);
            //await DefaultCategoryAssign(categorySvc, dbContextSvc);
            await SeedDemoUserAsync(userManagerSvc);
        }
        public static DateTime GetPostGresDate(DateTime datetime)
        {
            return DateTime.SpecifyKind(datetime, DateTimeKind.Utc);
        }

        private static async Task SeedDefaultUserAsync(UserManager<AppUser> userManagerSvc)
        {
            var defaultUser = new AppUser
            {
                UserName = "tonystark@mailinator.com",
                Email = "tonystark@mailinator.com",
                FirstName = "Tony",
                LastName = "Stark",
                EmailConfirmed = true,
            };
            try
            {
                var user = await userManagerSvc.FindByNameAsync(defaultUser.Email);
                if (user is null)
                {
                    await userManagerSvc.CreateAsync(defaultUser, "Abc&123!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("**** ERROR ****");
                Console.WriteLine("Error Seeding Default User");
                Console.WriteLine(ex.Message);
                Console.WriteLine("**** END OF ERROR ****");
            }
        }

        private static async Task SeedDefaultContacts(ApplicationDbContext dbContextSvc)
        {
            var userId = dbContextSvc.Users.FirstOrDefault(u => u.Email == "tonystark@mailinator.com")!.Id;

            var defaultContact = new Contact
            {
                UserId = userId,
                Created = DateTime.UtcNow,
                FirstName = "Henry",
                LastName = "McCoy",
                Address1 = "1407 Graymalkin Ln.",
                City = "Salem Center",
                PhoneNumber = "555-555-0101",
                State = "IL",
                ZipCode = "10560",
                EmailAddress = "hankmccoy@starktower.com"
            };
            try
            {
                var contact = await dbContextSvc.Contacts.AnyAsync(c => c.EmailAddress == "hankmccoy@starktower.com" && c.UserId == userId);
                if (!contact)
                {
                    await dbContextSvc.AddAsync(defaultContact);
                    await dbContextSvc.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("**** ERROR ****");
                Console.WriteLine("Error Seeding Default Contact");
                Console.WriteLine(ex.Message);
                Console.WriteLine("**** END OF ERROR ****");
            }

        }

        private static async Task SeedDefaultCategoriesAsync(ApplicationDbContext dbContextSvc)
        {
            var userId = dbContextSvc.Users.FirstOrDefault(u => u.Email == "tonystark@mailinator.com")!.Id;

            var defaultCategory = new Category
            {
                UserId = userId,
                Name = "X-Men"
            };
            try
            {
                var category = await dbContextSvc.Categories.AnyAsync(c => c.Name == "X-Men" && c.UserId == userId);
                if (!category)
                {
                    await dbContextSvc.AddAsync(defaultCategory);
                    await dbContextSvc.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("**** ERROR ****");
                Console.WriteLine("Error Seeding Default Category");
                Console.WriteLine(ex.Message);
                Console.WriteLine("**** END OF ERROR ****");
            }

        }

        private static async Task DefaultCategoryAssign(CategoryService categorySvc, ApplicationDbContext dbContextSvc)
        {
            var user = dbContextSvc.Users
                .Include(c => c.Categories)
                .Include(c => c.Contacts)
                .FirstOrDefault(u => u.Email == "tonystark@mailinator.com");
            var contact = dbContextSvc.Contacts
                .FirstOrDefault(u => u.EmailAddress == "hankmccoy@starktower.com");

            foreach (var category in user?.Categories!)
            {
                await categorySvc.AddContactToCategoriesAsync(category.Id, contact!.Id);
            }
        }
        public static async Task SeedDemoUserAsync(UserManager<AppUser> userManager)
        {
            //Seed DemoUser
            var demoUser = new AppUser
            {
                UserName = "demouser@mailinator.com",
                Email = "demouser@mailinator.com",
                FirstName = "Demo",
                LastName = "User",
                EmailConfirmed = true,
            };
            try
            {
                var user = await userManager.FindByEmailAsync(demoUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(demoUser, "Abc&123!");

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Demo User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }
        }


    }
}
