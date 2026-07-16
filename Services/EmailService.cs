using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using UretimPlanlama.Models;

namespace UretimPlanlama.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlMessage)
        {
            if (string.IsNullOrEmpty(_emailSettings.Server) || string.IsNullOrEmpty(_emailSettings.SenderEmail))
            {
                // Ayarlar yoksa veya boşsa mail gönderme
                return;
            }

            using (var client = new SmtpClient(_emailSettings.Server, _emailSettings.Port))
            {
                client.Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.Password);
                client.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };

                // Eğer birden fazla adres virgül ile ayrılmışsa:
                var recipients = to.Split(new[] { ',', ';' }, System.StringSplitOptions.RemoveEmptyEntries);
                foreach(var recipient in recipients)
                {
                    mailMessage.To.Add(recipient.Trim());
                }

                await client.SendMailAsync(mailMessage);
            }
        }
    }
}
