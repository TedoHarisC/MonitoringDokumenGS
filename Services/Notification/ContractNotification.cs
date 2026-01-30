using Dapper;
using Microsoft.EntityFrameworkCore;
using MonitoringDokumenGS.Data;
using MonitoringDokumenGS.Dtos;
using MonitoringDokumenGS.Interfaces;

public class ContractNotificationJob : IContractNotificationJob
{
    private readonly ApplicationDBContext _db;
    private readonly IEmailService _email;
    private readonly IAuditLog _audit;

    public ContractNotificationJob(
        ApplicationDBContext db,
        IEmailService email,
        IAuditLog audit)
    {
        _db = db;
        _email = email;
        _audit = audit;
    }

    public async Task RunAsync()
    {
        using var connection = _db.Database.GetDbConnection();

        var contracts = await connection.QueryAsync<ExpiringContractDto>(
            "SELECT * FROM V_Contract_ExpiringSoon");

        foreach (var c in contracts)
        {
            var body = $@"
            Contract {c.ContractNo} will expire in {c.DaysLeft} days.
            Please renew.";

            await _email.SendAsync(
                c.PicEmail,
                "Contract Expiring",
                body
            );

            await _audit.LogAsync(
                string.Empty,
                "CONTRACT_EXPIRY_REMINDER",
                "Contract",
                c.ContractId.ToString(),
                c.PicEmail
            );
        }
    }
}
