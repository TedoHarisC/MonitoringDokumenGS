using System.Threading.Tasks;

public interface IInvoiceNotificationJob
{
    Task RunAsync();
    Task RunTrialAsync(int templateDay, List<string>? overrideToEmails = null);
}
