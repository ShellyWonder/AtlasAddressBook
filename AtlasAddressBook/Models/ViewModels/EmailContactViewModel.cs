namespace AtlasAddressBook.Models.ViewModels
{
    public class EmailContactViewModel
    {
        //holds contact + the data coming from the Email Data class == multiple data sets
        public Contact? Contact { get; set; }
        public EmailData? EmailData { get; set; }
    }
}
