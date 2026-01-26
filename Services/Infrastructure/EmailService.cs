using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

public class SmtpEmailService : IEmailService
{
    private readonly EmailOptions _options;

    public SmtpEmailService(IOptions<EmailOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        var msg = new MailMessage();
        msg.From = new MailAddress(_options.FromEmail, _options.FromName);
        msg.To.Add(to);
        msg.Subject = subject;
        msg.Body = htmlBody; // EmailTemplate can be used here
        msg.IsBodyHtml = true;

        var client = new SmtpClient(_options.Smtp.Host, _options.Smtp.Port)
        {
            Credentials = new NetworkCredential(
                _options.Smtp.Username,
                _options.Smtp.Password
            ),
            EnableSsl = _options.Smtp.UseSsl
        };

        await client.SendMailAsync(msg);
    }

    private static string LoadTemplate(string name, Dictionary<string, string> data)
    {
        var path = Path.Combine("EmailTemplates", name);
        var html = File.ReadAllText(path);

        foreach (var item in data)
            html = html.Replace($"{{{{{item.Key}}}}}", item.Value);

        return html;
    }
}
