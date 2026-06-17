using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Adrenalin.SharedKernel.Interfaces;
using Microsoft.Extensions.Configuration;


namespace Adrenalin.Infrastructure.Email
{
    public class SmtpEmailService: IEmailService
    {
        private readonly EmailSettings _settings;
         public SmtpEmailService(
        IConfiguration configuration)
    {
        _settings =
            configuration
                .GetSection("Email")
                .Get<EmailSettings>()!;
    }
     public async Task SendAsync(
        string to,
        string subject,
        string body)
    {
        var email = new MimeMessage();

        email.From.Add(
            new MailboxAddress(
                _settings.DisplayName,
                _settings.From));

        email.To.Add(
            MailboxAddress.Parse(to));

        email.Subject = subject;
         email.Body =
            new TextPart("html")
            {
                Text = body
            };

        using var smtp = new SmtpClient();
        Console.WriteLine($"USER: {_settings.Username}");
Console.WriteLine($"PASS: {_settings.Password}");
Console.WriteLine($"HOST: {_settings.Host}");
Console.WriteLine($"PORT: {_settings.Port}");
        await smtp.ConnectAsync(
            _settings.Host,
            _settings.Port,
            SecureSocketOptions.StartTls);

        await smtp.AuthenticateAsync(
            _settings.Username,
            _settings.Password);

        await smtp.SendAsync(email);
         await smtp.DisconnectAsync(true);
    }
    }
}