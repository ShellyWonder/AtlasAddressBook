using System.ComponentModel.DataAnnotations;

namespace AtlasAddressBook.Models
{
    public class Category
    {
        //primary Key
        public int Id { get; set; }
        //Foreighn Key
        public string? UserId { get; set; }
        [Required]
        [Display(Name ="Category Name")]
        public string? Name { get; set; }
        //Navigation Properties (within db)
        public virtual AppUser? User { get; set; }

        public virtual ICollection<Contact> Contacts { get; set; } = new HashSet<Contact>();
       
    }
}
