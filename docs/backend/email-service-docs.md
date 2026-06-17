# Email Service — Developer Guide

**Project:** Adrenalin (FRESH_DESK 2.0 backend)  
**Namespace:** `Adrenalin.SharedKernel.Interfaces` / `Adrenalin.Infrastructure.Email`

---

## Overview

The email system is split into two independent concerns:

| Concern | Interface | Use case |
|---|---|---|
| **Sending** | `IEmailService` | Send transactional emails (welcome, OTP, password reset, etc.) |
| **Receiving** | `IEmailReceive` | Poll an inbox via IMAP and ingest emails as tickets |

Modules **only ever touch the interfaces** — never the concrete classes (`SmtpEmailService`, `ConsoleEmailLogger`, `ImapEmailReceiver`). All routing decisions (which provider fires) are handled by configuration, not code.

---

## Architecture

```
Your Handler / Command
        │
        ▼
  IEmailService          ← the only thing you inject
        │
        ▼
CompositeEmailService    ← reads "Email:Provider" from config
   ┌────┴────┐
   │         │
ConsoleEmail  SmtpEmail
Logger        Service
(dev/test)   (real send)
```

The `CompositeEmailService` is the sole registered implementation of `IEmailService`. It delegates to one or both backends based on the `Email:Provider` config value — no code change required to switch environments.

---

## Configuration (`appsettings.json`)

### Outbound (SMTP / Sending)

```json
{
  "Email": {
    "Provider": "console",
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "yourapp@gmail.com",
    "Password": "your-app-password",
    "From": "yourapp@gmail.com",
    "DisplayName": "Adrenalin Support"
  }
}
```

**`Provider` options:**

| Value | Behaviour |
|---|---|
| `"console"` | Prints email to the console in a formatted box. **Nothing is sent. Default.** |
| `"email"` or `"smtp"` | Sends via real SMTP. Nothing logged to console. |
| `"both"` | Logs to console **and** sends via SMTP. Useful for staging. |

> **Safe default:** If `Email:Provider` is missing or unknown, it falls back to `"console"` so misconfiguration never accidentally sends real emails.

### Inbound (IMAP / Receiving)

```json
{
  "EmailInbox": {
    "Enabled": true,
    "PollIntervalSeconds": 60,
    "Mode": "real",
    "Host": "imap.gmail.com",
    "Port": 993,
    "Username": "support@yourcompany.com",
    "Password": "your-imap-password"
  }
}
```

| Key | Default | Description |
|---|---|---|
| `Enabled` | `true` | Set to `false` to disable polling entirely |
| `PollIntervalSeconds` | `60` | Minimum 5 seconds |
| `Mode` | `"real"` | Set to `"simulate"` to use fake emails in dev without a real inbox |

---

## Dependency Injection Registration (`Program.cs`)

```csharp
// ── Outbound email ────────────────────────────────────────────────────────────
builder.Services.AddScoped<Adrenalin.Infrastructure.Email.SmtpEmailService>();
builder.Services.AddScoped<Adrenalin.Infrastructure.Email.ConsoleEmailLogger>();
builder.Services.AddScoped<IEmailService, Adrenalin.Infrastructure.Email.CompositeEmailService>();
// For full composite routing, register CompositeEmailService as IEmailService instead.

// ── Inbound email (IMAP polling) ──────────────────────────────────────────────
builder.Services.AddSingleton<IEmailReceive, ImapEmailReceiver>();
builder.Services.AddHostedService<EmailPollingJob>();
```

> `EmailPollingJob` is a `BackgroundService` that runs automatically on app start. It checks for new emails every `PollIntervalSeconds` and dispatches a `CreateTicketCommand` for each one.

---

## Sending Emails

### Step 1 — Inject `IEmailService`

```csharp
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.SharedKernel.Models;

public class MyCommandHandler : IRequestHandler<MyCommand, bool>
{
    private readonly IEmailService _emailService;

    public MyCommandHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }
    // ...
}
```

### Step 2 — Build an `EmailMessage` and call `SendAsync`

There are two ways to send:

#### Option A — Quick single-recipient (convenience overload)

```csharp
await _emailService.SendAsync(
    to: "user@example.com",
    subject: "Welcome to Adrenalin",
    htmlBody: "<h1>Hello!</h1><p>Your account is ready.</p>"
);
```

#### Option B — Full `EmailMessage` (To / Cc / Bcc / multiple recipients)

```csharp
var message = new EmailMessage(
    to:      "alice@example.com;bob@example.com",  // semicolons or commas both work
    subject: "Ticket #1042 — Status Update",
    body:    "<p>Your ticket has been resolved.</p>",
    cc:      "manager@example.com",
    bcc:     "audit@example.com"
);

await _emailService.SendAsync(message, cancellationToken);
```

#### Option C — Object initialiser (when building list dynamically)

```csharp
var message = new EmailMessage
{
    To      = new List<string> { "alice@example.com", "bob@example.com" },
    Cc      = new List<string> { "manager@example.com" },
    Subject = "Scheduled Maintenance Tonight",
    Body    = "<p>Systems will be down from 11 PM – 1 AM.</p>"
};

await _emailService.SendAsync(message, cancellationToken);
```

---

## Real-World Examples from the Codebase

### 1. Welcome email with password-set link (External User Created)

```csharp
// CreateExternalUserCommandHandler.cs
var resetLink = $"http://localhost:5088/reset-password?token={rawToken}";

await _emailService.SendAsync(
    user.Email,
    "Set Your Password",
    $@"
    <h2>Welcome to Adrenalin</h2>
    <p>Your account has been created.</p>
    <p>
        <a href='{resetLink}'>Click here to set your password</a>
    </p>
    <p>This link expires in 24 hours.</p>
    "
);
```

### 2. Forgot Password — reset link

```csharp
// ForgotPasswordCommandHandler.cs
var resetUrl = $"http://localhost:5088/api/auth/reset-password?token={rawToken}";

await _emailService.SendAsync(
    user.Email,
    "Set Your Password",
    $@"
    <h2>Password Reset</h2>
    <p>
        <a href='{resetUrl}'>Click here to reset your password</a>
    </p>
    <p>This link expires in 1 hour.</p>
    "
);
```

### 3. OTP / Email Verification

```csharp
// RegisterUserCommandHandler.cs
await _emailService.SendAsync(
    user.Email,
    "Email Verification OTP",
    $"Your OTP is {otp}"
);
```

### 4. Resend Verification Link

```csharp
// ResendVerificationCommandHandler.cs
var verifyUrl = $"http://localhost:5088/api/auth/verify-email?token={rawToken}";

await _emailService.SendAsync(
    user.Email,
    "Verify Your Email",
    $@"
    <h2>Email Verification</h2>
    <p>
        <a href='{verifyUrl}'>Verify Email</a>
    </p>
    "
);
```

---

## `EmailMessage` Reference

```csharp
public sealed class EmailMessage
{
    public List<string> To      { get; init; }   // required
    public List<string> Cc      { get; init; }   // optional
    public List<string> Bcc     { get; init; }   // optional
    public string       Subject { get; init; }   // required
    public string       Body    { get; init; }   // HTML string, required
}
```

**Multi-address shorthand:** The constructor accepts `"a@x.com;b@x.com"` or `"a@x.com,b@x.com"` as a single string for `To`/`Cc`/`Bcc` — it splits automatically on `;` or `,`.

---

## Receiving Emails (IMAP)

You do **not** need to interact with `IEmailReceive` directly in most cases. The `EmailPollingJob` background service handles it automatically:

1. Every `PollIntervalSeconds`, it calls `IEmailReceive.ReceiveEmailsAsync()`.
2. Each `InboundEmail` is cleaned via `EmailParser` and dispatched as a `CreateTicketCommand`.

### `InboundEmail` record (what the receiver returns)

```csharp
public sealed record InboundEmail(
    string SenderEmail,
    string SenderName,
    string Subject,
    string Body
);
```

### If you need to consume `IEmailReceive` directly

```csharp
public class MyService
{
    private readonly IEmailReceive _inbox;

    public MyService(IEmailReceive inbox) => _inbox = inbox;

    public async Task ProcessInboxAsync(CancellationToken ct)
    {
        var emails = await _inbox.ReceiveEmailsAsync(ct);

        foreach (var email in emails)
        {
            Console.WriteLine($"From: {email.SenderName} <{email.SenderEmail}>");
            Console.WriteLine($"Subject: {email.Subject}");
            Console.WriteLine($"Body: {email.Body}");
        }
    }
}
```

### Simulation mode (dev / testing)

Set `EmailInbox:Mode` to anything other than `"real"` (or omit `Host`/`Username`) and `ImapEmailReceiver` returns hardcoded fake emails in rotation:

| Cycle | Simulated sender |
|---|---|
| 1 | `rahul@gmail.com` — existing customer |
| 2 | `new.employee@abc.org` — new contact, known domain |
| 3 | `service@spacex.com` — completely unknown sender |
| 4+ | Empty (no new emails) |

---

## `EmailParser` Helper (Static)

Used internally by `EmailPollingJob`, but available anywhere:

```csharp
using Adrenalin.Infrastructure.Email;

// Strip reply chains, signatures, and noise from an email subject
string cleanTitle = EmailParser.CleanSubject(email.Subject);

// Strip HTML tags, trim whitespace, shorten body to ticket-appropriate length
string cleanBody  = EmailParser.CleanBody(email.Body);
```

---

## Development Workflow

### Local dev — console mode (default)

Set `Email:Provider` to `"console"`. Every email is printed to your terminal:

```
╔══════════════════════════════════════════════════════════════════════════════╗
║       📧  EMAIL NOTIFICATION (console mode — not actually sent)             ║
╠══════════════════════════════════════════════════════════════════════════════╣
║ From    : Adrenalin Support <yourapp@gmail.com>                             ║
║ To      : alice@example.com                                                  ║
║ Subject : Verify Your Email                                                  ║
╟──────────────────────────────────────────────────────────────────────────────╢
║                                  BODY                                        ║
╟──────────────────────────────────────────────────────────────────────────────╢
║  Email Verification                                                          ║
║  Verify Email → http://localhost:5088/api/auth/verify-email?token=abc123    ║
╚══════════════════════════════════════════════════════════════════════════════╝
```

### Staging — both modes

```json
"Email": { "Provider": "both" }
```

Logs to console **and** sends real emails. Good for catching what actually goes out before going to production.

### Production — SMTP only

```json
"Email": { "Provider": "email" }
```

---

## Quick Reference

| Task | What to do |
|---|---|
| Send a simple email | `await _emailService.SendAsync(to, subject, htmlBody)` |
| Send with Cc / Bcc | Create an `EmailMessage` object and pass to `SendAsync` |
| Switch to real SMTP | Change `Email:Provider` to `"email"` in `appsettings.json` |
| Disable inbound polling | Set `EmailInbox:Enabled` to `false` |
| Test without real inbox | Set `EmailInbox:Mode` to `"simulate"` |
| Don't directly inject | `SmtpEmailService`, `ConsoleEmailLogger`, or `ImapEmailReceiver` |
| Always inject | `IEmailService` (outbound) or `IEmailReceive` (inbound) |
