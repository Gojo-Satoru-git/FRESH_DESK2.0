using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.SharedKernel.Models;

namespace Adrenalin.Infrastructure.Email
{
    /// <summary>
    /// Real SMTP sender, built on MailKit/MimeKit (free, open-source — no paid
    /// provider needed). Works with Gmail SMTP, Mailtrap, or any standard SMTP host
    /// configured under the "Email" section in appsettings.json.
    /// </summary>
    public sealed class SmtpEmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public SmtpEmailService(IConfiguration configuration)
        {
            _settings = configuration.GetSection("Email").Get<EmailSettings>()
                         ?? new EmailSettings();
             Console.WriteLine("=== EMAIL SETTINGS ===");
    Console.WriteLine($"Host: {_settings.Host}");
    Console.WriteLine($"Port: {_settings.Port}");
    Console.WriteLine($"User: {_settings.Username}");
    Console.WriteLine($"Pass: {_settings.Password}");
    Console.WriteLine($"From: {_settings.From}");
        }

        public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            var email = new MimeMessage();

            email.From.Add(new MailboxAddress(_settings.DisplayName, _settings.From));

            foreach (var to in message.To)
                email.To.Add(MailboxAddress.Parse(to));

            foreach (var cc in message.Cc)
                email.Cc.Add(MailboxAddress.Parse(cc));

            foreach (var bcc in message.Bcc)
                email.Bcc.Add(MailboxAddress.Parse(bcc));

            email.Subject = message.Subject;
            email.Body = new TextPart("html") { Text = message.Body };

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                _settings.Host,
                _settings.Port,
                SecureSocketOptions.StartTls,
                cancellationToken);
            Console.WriteLine($"SMTP USER = {_settings.Username}");
                Console.WriteLine($"SMTP PASS = {_settings.Password}");
            await smtp.AuthenticateAsync(
                
                _settings.Username,
                _settings.Password,
                cancellationToken);

            await smtp.SendAsync(email, cancellationToken);
            await smtp.DisconnectAsync(true, cancellationToken);
        }
    }
}
