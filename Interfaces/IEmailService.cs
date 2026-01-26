namespace MonitoringDokumenGS.Interfaces
{
    /// <summary>
    /// Interface untuk Email Service
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Send email to single recipient
        /// </summary>
        Task SendAsync(string to, string subject, string htmlBody);

        /// <summary>
        /// Send email to multiple recipients
        /// </summary>
        Task SendToMultipleAsync(List<string> toAddresses, string subject, string htmlBody);

        /// <summary>
        /// Send email with CC and BCC
        /// </summary>
        Task SendWithCopyAsync(string to, string subject, string htmlBody, List<string>? cc = null, List<string>? bcc = null);
    }
}
