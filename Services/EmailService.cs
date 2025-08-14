using MimeKit;
using MailKit.Net.Smtp;

using Microsoft.Extensions.Configuration;
namespace FaizHesaplamaAPI.Services
{
    public interface IEmailService
    {
        Task SendVerificationCodeAsync(string email , string verificationCode);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendVerificationCodeAsync(string email, string verificationCode)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(emailSettings["SenderName"], emailSettings["SenderEmail"]));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = "Email Doğrulama Kodu";
            message.Body = new TextPart("plain")
            {
                Text =  $"Doğrulama kodunuz: {verificationCode}\n\n" + $"Bu kod 3 dakika geçerlidir." //Verification Service'de 3 dakika geçerli olacak şekilde ayarladım(TimeSpan)
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(emailSettings["MailServer"], int.Parse(emailSettings["MailPort"]),
                bool.Parse(emailSettings["UseSsl"]));
            await client.AuthenticateAsync(emailSettings["Username"] , emailSettings["Password"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

        }

    }
}
