using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AtlasAddressBook.Models
{
    public class EmailData
    {
        [Required]
        public string EmailAddress { get; set; } = string.Empty;
        [Required]
        public string Subject { get; set; } = string.Empty;
        [Required]
        public string Body { get; set; } = string.Empty;

        public int? id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set;}
        public string? GroupName { get; set; }

        [NotMapped]
        public string? FullName { get { return $"{FirstName} {LastName}"; } }
    }
}
