public class EmailOptions
{
    public string Provider { get; set; } = "SMTP";
    public string FromName { get; set; } = default!;
    public string FromEmail { get; set; } = default!;
    public SmtpOptions Smtp { get; set; } = new();
}

public class SmtpOptions
{
    public string Host { get; set; } = default!;
    public int Port { get; set; }
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
    public bool UseSsl { get; set; }
}
