using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.SharedKernel.Models;

namespace Adrenalin.Infrastructure.Email
{
    /// <summary>
    /// THE single IEmailService implementation that every module injects.
    ///
    /// Modules never reference SmtpEmailService or ConsoleEmailLogger directly —
    /// they only depend on IEmailService and call SendAsync(EmailMessage). This
    /// class reads "Email:Provider" from config and decides, at send time, which
    /// underlying sender(s) to invoke:
    ///
    ///   "console" -> ConsoleEmailLogger only   (prints to console, nothing sent)
    ///   "email"   -> SmtpEmailService only      (real SMTP send)
    ///   "both"    -> ConsoleEmailLogger + SmtpEmailService
    ///
    /// To change behavior, just edit appsettings.json — no code changes needed.
    /// </summary>
    public sealed class CompositeEmailService : IEmailService
    {
        private readonly ConsoleEmailLogger _consoleLogger;
        private readonly SmtpEmailService _smtpSender;
        private readonly ILogger<CompositeEmailService> _logger;
        private readonly string _provider;

        public CompositeEmailService(
            ConsoleEmailLogger consoleLogger,
            SmtpEmailService smtpSender,
            IConfiguration configuration,
            ILogger<CompositeEmailService> logger)
        {
            _consoleLogger = consoleLogger;
            _smtpSender = smtpSender;
            _logger = logger;
            _provider = configuration["Email:Provider"]?.Trim().ToLowerInvariant() ?? "console";
        }

        public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            switch (_provider)
            {
                case "email":
                case "smtp":
                    await _smtpSender.SendAsync(message, cancellationToken);
                    break;

                case "both":
                    // Log to console first so the email is always visible locally,
                    // then attempt the real send.
                    await _consoleLogger.SendAsync(message, cancellationToken);
                    await _smtpSender.SendAsync(message, cancellationToken);
                    break;

                case "console":
                    await _consoleLogger.SendAsync(message, cancellationToken);
                    break;

                default:
                    _logger.LogWarning(
                        "Unknown Email:Provider '{Provider}' — falling back to console logging.",
                        _provider);
                    await _consoleLogger.SendAsync(message, cancellationToken);
                    break;
            }
        }
    }
}
