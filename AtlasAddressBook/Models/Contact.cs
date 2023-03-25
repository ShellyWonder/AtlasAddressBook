using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AtlasAddressBook.Enums;


namespace AtlasAddressBook.Models
{
    public class Contact
    {
        public int Id { get; set; }

        [Required]
        public string? UserId { get; set; }

        [Required]
        [Display(Name = "First Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        public string? FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        public string? LastName { get; set; }

        [NotMapped]
        public string FullName { get { return $"{FirstName} {LastName}"; } }


        [DataType(DataType.Date)]
        public DateTime? Birthday { get; set; }

        [Required]
        [StringLength(75, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Address")]
        public string? Address1 { get; set; }

        [StringLength(75, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 0)]
        public string? Address2 { get; set; }

        [Required]
        public string? City { get; set; }

        [Required]
        public string? State { get; set; }

        [Required]
        [DataType(DataType.PostalCode)]
        [RegularExpression(@"^(?!0{5}|[0-9]{9})[0-9]{5}(?:-[0-9]{4})?$", ErrorMessage = "Postal code must be 5 or 9 digits and must not contain all zeros. If 9 digits, it must not begin with 5 zeros or end in 4 zeros.")]
        [Display(Name = "Zip Code")]
        public string? ZipCode { get; set; }


        [Required]
        [DataType(DataType.EmailAddress)]
        public string? EmailAddress { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string? PhoneNumber { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime Created { get; set; }


        [NotMapped]
        [DataType(DataType.Upload)]
        [Display(Name = "Contact Image")]
        public IFormFile? ImageFile { get; set; }
        public byte[]? ImageData { get; set; }
        public string? ImageType { get; set; }

        //NAVIGATION PROPERTIES
        public virtual AppUser? User { get; set; }

        //creates a join table between Contact and Category 
        public virtual ICollection<Category> Categories { get; set; } = new HashSet<Category>();
    }
}
