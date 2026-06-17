using System.Collections.Generic;
using System.Linq;

namespace Adrenalin.SharedKernel.Models
{
    /// <summary>
    /// Single payload used everywhere an email needs to be sent.
    /// Build one of these and pass it to <see cref="Interfaces.IEmailService.SendAsync(EmailMessage, System.Threading.CancellationToken)"/>.
    /// Every module uses this ONE method — the active provider (console / smtp / both)
    /// is purely a config concern handled inside the Infrastructure layer.
    /// </summary>
    public sealed class EmailMessage
    {
        public List<string> To { get; init; } = new();
        public List<string> Cc { get; init; } = new();
        public List<string> Bcc { get; init; } = new();
        public string Subject { get; init; } = string.Empty;

        /// <summary>HTML body.</summary>
        public string Body { get; init; } = string.Empty;

        public EmailMessage() { }

        public EmailMessage(
            string to,
            string subject,
            string body,
            string? cc = null,
            string? bcc = null)
        {
            To = SplitAddresses(to);
            Cc = SplitAddresses(cc);
            Bcc = SplitAddresses(bcc);
            Subject = subject;
            Body = body;
        }

        /// <summary>
        /// Lets callers pass "a@x.com;b@x.com" or "a@x.com,b@x.com" as a single string
        /// for To/Cc/Bcc instead of building a List&lt;string&gt; every time.
        /// </summary>
        private static List<string> SplitAddresses(string? addresses)
        {
            if (string.IsNullOrWhiteSpace(addresses))
                return new List<string>();

            return addresses
                .Split(new[] { ';', ',' }, System.StringSplitOptions.RemoveEmptyEntries)
                .Select(a => a.Trim())
                .Where(a => a.Length > 0)
                .ToList();
        }
    }
}
