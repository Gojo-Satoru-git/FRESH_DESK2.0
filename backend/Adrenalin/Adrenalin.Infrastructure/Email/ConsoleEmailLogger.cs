using System;
using System.Threading;
using System.Threading.Tasks;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.SharedKernel.Models;

namespace Adrenalin.Infrastructure.Email
{
    /// <summary>
    /// Dev-friendly "sender" that prints the email to the console inside a bordered
    /// template instead of actually sending it. Useful for local development so you
    /// don't need a real SMTP server running to see what would have been sent.
    /// </summary>
    public sealed class ConsoleEmailLogger : IEmailService
    {
        private readonly EmailSettings _settings;

        public ConsoleEmailLogger(Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _settings = configuration.GetSection("Email").Get<EmailSettings>()
                         ?? new EmailSettings();
        }

        public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            const int width = 78;
            var from = string.IsNullOrWhiteSpace(_settings.DisplayName)
                ? _settings.From
                : $"{_settings.DisplayName} <{_settings.From}>";

            var originalColor = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔" + new string('═', width) + "╗");
            WriteCentered("📧  EMAIL NOTIFICATION (console mode — not actually sent)", width);
            Console.WriteLine("╠" + new string('═', width) + "╣");

            WriteField("From", from, width);
            WriteField("To", Join(message.To), width);
            WriteField("Cc", Join(message.Cc), width);
            WriteField("Bcc", Join(message.Bcc), width);
            WriteField("Subject", message.Subject, width);

            Console.WriteLine("╟" + new string('─', width) + "╢");
            WriteCentered("BODY", width);
            Console.WriteLine("╟" + new string('─', width) + "╢");

            foreach (var line in WrapBody(message.Body, width - 4))
                WriteLine(" " + line, width);

            Console.WriteLine("╚" + new string('═', width) + "╝");
            Console.ForegroundColor = originalColor;

            return Task.CompletedTask;
        }

        private static string Join(System.Collections.Generic.List<string> values)
            => values.Count == 0 ? "-" : string.Join(", ", values);

        private static void WriteField(string label, string value, int width)
        {
            WriteLine($" {label,-8}: {value}", width);
        }

        private static void WriteLine(string content, int width)
        {
            if (content.Length > width)
                content = content[..(width - 1)] + "…";

            Console.WriteLine("║" + content.PadRight(width) + "║");
        }

        private static void WriteCentered(string content, int width)
        {
            var padding = Math.Max(0, (width - content.Length) / 2);
            var line = new string(' ', padding) + content;
            Console.WriteLine("║" + line.PadRight(width) + "║");
        }

        /// <summary>
        /// Minimal HTML-strip + word-wrap so an HTML email body still reads cleanly
        /// inside the fixed-width console box.
        /// </summary>
        private static System.Collections.Generic.IEnumerable<string> WrapBody(string html, int maxWidth)
        {
            var text = System.Text.RegularExpressions.Regex.Replace(html ?? string.Empty, "<.*?>", string.Empty);
            text = System.Net.WebUtility.HtmlDecode(text).Trim();

            if (string.IsNullOrEmpty(text))
            {
                yield return "(empty body)";
                yield break;
            }

            foreach (var rawLine in text.Split('\n'))
            {
                var words = rawLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var current = string.Empty;

                foreach (var word in words)
                {
                    if ((current + " " + word).Trim().Length > maxWidth)
                    {
                        if (current.Length > 0)
                            yield return current.Trim();
                        current = word;
                    }
                    else
                    {
                        current = (current + " " + word).Trim();
                    }
                }

                if (current.Length > 0)
                    yield return current;
                else if (string.IsNullOrWhiteSpace(rawLine))
                    yield return string.Empty;
            }
        }
    }
}
