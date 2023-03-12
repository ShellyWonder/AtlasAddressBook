namespace AtlasAddressBook.Models
{
    public class MailSettings
    {
        //mimicks object properties in Mail Settings in user secrets
        public string? Email { get; set; }
        public string? Password  { get; set; }
        public string? DisplayName { get; set; }
        public string? Host { get; set; }
        public int Port { get; set; }

    }
}
