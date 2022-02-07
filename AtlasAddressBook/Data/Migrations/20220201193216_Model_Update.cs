using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtlasAddressBook.Data.Migrations
{
    public partial class Model_Update : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategoryContact_Contacts_ContactsContactId",
                table: "CategoryContact");

            migrationBuilder.RenameColumn(
                name: "created",
                table: "Contacts",
                newName: "Created");

            migrationBuilder.RenameColumn(
                name: "ContactId",
                table: "Contacts",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ContactsContactId",
                table: "CategoryContact",
                newName: "ContactsId");

            migrationBuilder.RenameIndex(
                name: "IX_CategoryContact_ContactsContactId",
                table: "CategoryContact",
                newName: "IX_CategoryContact_ContactsId");

            migrationBuilder.AddForeignKey(
                name: "FK_CategoryContact_Contacts_ContactsId",
                table: "CategoryContact",
                column: "ContactsId",
                principalTable: "Contacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategoryContact_Contacts_ContactsId",
                table: "CategoryContact");

            migrationBuilder.RenameColumn(
                name: "Created",
                table: "Contacts",
                newName: "created");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Contacts",
                newName: "ContactId");

            migrationBuilder.RenameColumn(
                name: "ContactsId",
                table: "CategoryContact",
                newName: "ContactsContactId");

            migrationBuilder.RenameIndex(
                name: "IX_CategoryContact_ContactsId",
                table: "CategoryContact",
                newName: "IX_CategoryContact_ContactsContactId");

            migrationBuilder.AddForeignKey(
                name: "FK_CategoryContact_Contacts_ContactsContactId",
                table: "CategoryContact",
                column: "ContactsContactId",
                principalTable: "Contacts",
                principalColumn: "ContactId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
