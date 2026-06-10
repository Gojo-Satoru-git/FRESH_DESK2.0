using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.SharedKernel.Interfaces;

namespace Adrenalin.Infrastructure.Email
{
    public class FakeEmailService: IEmailService
    {
        public Task SendAsync(
        string to,
        string subject,
        string htmlBody)
    {
        Console.WriteLine("========== EMAIL ==========");
        Console.WriteLine($"To: {to}");
        Console.WriteLine($"Subject: {subject}");
        Console.WriteLine(htmlBody);
        Console.WriteLine("===========================");

        return Task.CompletedTask;
    }
    }
}