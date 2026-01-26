namespace MonitoringDokumenGS.Models
{
    /// <summary>
    /// Email configuration options dari appsettings.json
    /// </summary>
    public class EmailOptions
    {
        public string Provider { get; set; } = "SMTP";
        public string FromName { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public SmtpOptions Smtp { get; set; } = new SmtpOptions();
    }

    public class SmtpOptions
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool UseSsl { get; set; } = true;
    }
}
