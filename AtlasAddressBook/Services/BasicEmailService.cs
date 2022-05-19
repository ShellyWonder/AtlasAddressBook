using MailKit.Security;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace AtlasAddressBook.Services
{
    public class BasicEmailService : IEmailSender
    {

        private readonly IConfiguration _appSettings;


        public BasicEmailService(IConfiguration appSettings)
        {
            _appSettings = appSettings;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailSender = _appSettings["SmtpSettings:UserName"];

            MimeMessage newEmail = new();

            newEmail.Sender = MailboxAddress.Parse(emailSender);
            newEmail.To.Add(MailboxAddress.Parse(email));
            newEmail.Subject = subject;

            //Email Body
            BodyBuilder emailBody = new();
            emailBody.HtmlBody = htmlMessage;
            newEmail.Body = emailBody.ToMessageBody();

            //configuring the SMTP server
            using MailKit.Net.Smtp.SmtpClient smtpClient = new();

            var host = _appSettings["SmtpSettings:Host"];
            var port = Convert.ToInt32(_appSettings["SmtpSettings:Port"]);

            await smtpClient.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            await smtpClient.AuthenticateAsync(emailSender, _appSettings["SmtpSettings:Password"]);
            await smtpClient.SendAsync(newEmail);
            await smtpClient.DisconnectAsync(true);

        }
    }
}
