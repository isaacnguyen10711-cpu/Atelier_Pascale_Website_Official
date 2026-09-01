using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace AtelierPascaleWebsite.Services
{
    public class EmailSender
    {
        private readonly IConfiguration _configuration;
        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        // Method to send an email
        public async Task SendEmailAsync(string receiverName, string receiverEmail, string subject, string body)
        {
            var smtpHost = _configuration["EmailSettings:SmtpServer"]!;
            var smtpPort = int.Parse(_configuration["EmailSettings:Port"]!);
            var senderName = _configuration["EmailSettings:SenderName"]!;
            var senderEmail = _configuration["EmailSettings:SenderEmail"]!;
            var smtpPass = _configuration["EmailSettings:Password"]! ;

            // Create the email message
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(senderName, senderEmail));
            email.To.Add(new MailboxAddress(receiverName, receiverEmail));
            email.Subject = subject;
            email.Body = new TextPart("html") { Text = body };

            // Create the SMTP client and send the email
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(senderEmail, smtpPass);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
