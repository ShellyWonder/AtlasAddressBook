using AtlasAddressBook.Data;
using AtlasAddressBook.Enums;
using AtlasAddressBook.Models;
using AtlasAddressBook.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AtlasAddressBook.Helpers
{
    //ensures the db always remains insync; equivalent to update-database in deployment

    public static class DataHelper
    {
        
        //IServiceProvider hooks to all services

            public static async Task ManageDataAsync(IServiceProvider svcProvider)
            {
                //creates instance of db application context
                var dbContextSvc = svcProvider.GetRequiredService<ApplicationDbContext>();
                //same functionality as update-database
                await dbContextSvc.Database.MigrateAsync();

            }


    }

        
}

