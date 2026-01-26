using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using MonitoringDokumenGS.Interfaces;
using MonitoringDokumenGS.Models;

namespace MonitoringDokumenGS.Services.Infrastructure
{
    /// <summary>
    /// SMTP Email Service Implementation
    /// </summary>
    public class SmtpEmailService : IEmailService
    {
        private readonly EmailOptions _options;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IOptions<EmailOptions> options, ILogger<SmtpEmailService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        /// <summary>
        /// Send email to single recipient
        /// </summary>
        public async Task SendAsync(string to, string subject, string htmlBody)
        {
            try
            {
                var msg = new MailMessage();
                msg.From = new MailAddress(_options.FromEmail, _options.FromName);
                msg.To.Add(to);
                msg.Subject = subject;
                msg.Body = htmlBody;
                msg.IsBodyHtml = true;

                using var client = new SmtpClient(_options.Smtp.Host, _options.Smtp.Port)
                {
                    Credentials = new NetworkCredential(
                        _options.Smtp.Username,
                        _options.Smtp.Password
                    ),
                    EnableSsl = _options.Smtp.UseSsl
                };

                await client.SendMailAsync(msg);
                _logger.LogInformation("Email sent successfully to {To} with subject: {Subject}", to, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To} with subject: {Subject}", to, subject);
                throw;
            }
        }

        /// <summary>
        /// Send email to multiple recipients
        /// </summary>
        public async Task SendToMultipleAsync(List<string> toAddresses, string subject, string htmlBody)
        {
            try
            {
                var msg = new MailMessage();
                msg.From = new MailAddress(_options.FromEmail, _options.FromName);

                foreach (var email in toAddresses)
                {
                    msg.To.Add(email);
                }

                msg.Subject = subject;
                msg.Body = htmlBody;
                msg.IsBodyHtml = true;

                using var client = new SmtpClient(_options.Smtp.Host, _options.Smtp.Port)
                {
                    Credentials = new NetworkCredential(
                        _options.Smtp.Username,
                        _options.Smtp.Password
                    ),
                    EnableSsl = _options.Smtp.UseSsl
                };

                await client.SendMailAsync(msg);
                _logger.LogInformation("Email sent successfully to {Count} recipients with subject: {Subject}",
                    toAddresses.Count, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to multiple recipients with subject: {Subject}", subject);
                throw;
            }
        }

        /// <summary>
        /// Send email with CC and BCC
        /// </summary>
        public async Task SendWithCopyAsync(string to, string subject, string htmlBody,
            List<string>? cc = null, List<string>? bcc = null)
        {
            try
            {
                var msg = new MailMessage();
                msg.From = new MailAddress(_options.FromEmail, _options.FromName);
                msg.To.Add(to);

                if (cc != null)
                {
                    foreach (var email in cc)
                    {
                        msg.CC.Add(email);
                    }
                }

                if (bcc != null)
                {
                    foreach (var email in bcc)
                    {
                        msg.Bcc.Add(email);
                    }
                }

                msg.Subject = subject;
                msg.Body = htmlBody;
                msg.IsBodyHtml = true;

                using var client = new SmtpClient(_options.Smtp.Host, _options.Smtp.Port)
                {
                    Credentials = new NetworkCredential(
                        _options.Smtp.Username,
                        _options.Smtp.Password
                    ),
                    EnableSsl = _options.Smtp.UseSsl
                };

                await client.SendMailAsync(msg);
                _logger.LogInformation("Email sent successfully to {To} with CC/BCC", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email with CC/BCC to {To}", to);
                throw;
            }
        }
    }
}
