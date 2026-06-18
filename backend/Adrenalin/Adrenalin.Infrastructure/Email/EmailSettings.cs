namespace Adrenalin.Infrastructure.Email
{
    /// <summary>
    /// Bound from the "Email" section of appsettings.json.
    /// </summary>
    public class EmailSettings
    {
        /// <summary>
        /// Which sender(s) to use: "console", "email" (smtp), or "both".
        /// Defaults to "console" so a misconfigured/missing setting never
        /// accidentally tries to send real emails.
        /// </summary>
        public string Provider { get; set; } = "console";

        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }
}
