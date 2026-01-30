using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "SUPER_ADMIN,ADMIN")]
[ApiController]
[Route("api/jobs")]
public class NotificationJobApiController : ControllerBase
{
    private readonly IContractNotificationJob _job;
    private readonly ILogger<NotificationJobApiController> _logger;

    public NotificationJobApiController(
        IContractNotificationJob job,
        ILogger<NotificationJobApiController> logger)
    {
        _job = job;
        _logger = logger;
    }

    /// <summary>
    /// Run contract expiry notification job manually.
    /// </summary>
    [HttpPost("contract-expiry")]
    public async Task<IActionResult> RunContractExpiry()
    {
        try
        {
            await _job.RunAsync();
            return Ok(new
            {
                success = true,
                message = "Contract expiry notification job executed successfully."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running contract expiry job");

            return StatusCode(500, new
            {
                success = false,
                message = "Failed to execute contract expiry notification job."
            });
        }
    }
}
