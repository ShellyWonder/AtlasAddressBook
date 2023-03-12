using MailKit.Security;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using AtlasAddressBook.Models;

namespace AtlasAddressBook.Services
{
    public class BasicEmailService : IEmailSender
    {
        private readonly MailSettings _mailSettings;
        #region Constructor
        //IOptions necessary to pass values from mailSettings
        public BasicEmailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }
        #endregion
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            //emailSender allows method to login to sender email account to send email
            var emailSender = _mailSettings.Email ?? Environment.GetEnvironmentVariable("Email");
            MimeMessage newEmail = new();

            //sender's email
            newEmail.Sender = MailboxAddress.Parse(emailSender);

            //recipient's email address
            foreach (var emailAddress in email.Split(";"))
            {
                newEmail.To.Add(MailboxAddress.Parse(emailAddress));
            }
            newEmail.Subject = subject;

            //create email message
            BodyBuilder emailBody = new();
            emailBody.HtmlBody = htmlMessage;
            newEmail.Body = emailBody.ToMessageBody();

            using SmtpClient smtpClient = new SmtpClient();

            try
            {
                var host = _mailSettings.Host ?? Environment.GetEnvironmentVariable("Host");

                //Parse necessary because without parse port is recognized as a string, needs to be int.
                var port = _mailSettings.Port != 0 ? _mailSettings.Port : int.Parse(Environment.GetEnvironmentVariable("Port")!);
                var password = _mailSettings.Password ?? Environment.GetEnvironmentVariable("Password");
                //connect to sender's email
                await smtpClient.ConnectAsync(host, port, SecureSocketOptions.StartTls);
                //Auth sender's email
                await smtpClient.AuthenticateAsync(emailSender, password);
                //send email to recepient 
                await smtpClient.SendAsync(newEmail);
                //disconnect from sender's email
                await smtpClient.DisconnectAsync(true);

            }
            catch (Exception ex)
            {
                var error = ex.Message;
                throw;
            }

        }
    }
}

