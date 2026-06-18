using System.Threading;
using System.Threading.Tasks;
using Adrenalin.SharedKernel.Models;

namespace Adrenalin.SharedKernel.Interfaces
{
    /// <summary>
    /// The ONE email contract every module depends on. Modules never know or care
    /// whether the active provider is Console, SMTP, or Both — that routing decision
    /// lives entirely behind this interface (see CompositeEmailService in Infrastructure).
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// The single send method. Build an <see cref="EmailMessage"/> (To/Cc/Bcc/Subject/Body)
        /// and call this — that's it, regardless of which provider(s) are configured.
        /// </summary>
        Task SendAsync(
            EmailMessage message,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Back-compat convenience overload for existing handlers written against the
        /// old 3-arg signature. Just forwards into <see cref="EmailMessage"/> + SendAsync above,
        /// so no existing call site needs to change.
        /// </summary>
        Task SendAsync(string to, string subject, string htmlBody)
            => SendAsync(new EmailMessage(to, subject, htmlBody));
    }
}
