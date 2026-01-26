using Microsoft.AspNetCore.Mvc;
using MonitoringDokumenGS.Services.Infrastructure;
using System;

namespace MonitoringDokumenGS.Controllers.API
{
    /// <summary>
    /// Test controller untuk preview email templates
    /// </summary>
    [ApiController]
    [Route("api/test/email")]
    public class EmailPreviewController : ControllerBase
    {
        /// <summary>
        /// Preview email template
        /// GET api/test/email/preview/{template}
        /// </summary>
        [HttpGet("preview/{template}")]
        public IActionResult PreviewTemplate(string template)
        {
            try
            {
                string html = template.ToLower() switch
                {
                    "invoice" => EmailTemplateHelper.GetInvoiceCreatedEmail(
                        invoiceNumber: "INV-2026-001",
                        vendorName: "PT. Vendor ABC Indonesia",
                        amount: "Rp 50,000,000",
                        dueDate: "February 15, 2026",
                        status: "Pending Approval",
                        createdBy: "John Doe",
                        createdDate: "January 26, 2026",
                        description: "Payment for IT services and software licenses rendered in January 2026. This includes monthly subscription and support fees.",
                        invoiceLink: "http://localhost:5170/invoice/123"
                    ),

                    "contract" => EmailTemplateHelper.GetContractCreatedEmail(
                        contractNumber: "CON-2026-005",
                        vendorName: "PT. Supplier XYZ Corporation",
                        contractValue: "Rp 250,000,000",
                        startDate: "February 1, 2026",
                        endDate: "December 31, 2026",
                        status: "Active",
                        createdBy: "Jane Smith",
                        createdDate: "January 26, 2026",
                        description: "Annual maintenance contract for all office equipment including computers, printers, and network infrastructure.",
                        contractLink: "http://localhost:5170/contract/456"
                    ),

                    "welcome" => EmailTemplateHelper.GetWelcomeEmail(
                        userName: "John Doe",
                        username: "johndoe",
                        email: "john.doe@abb.com",
                        role: "Admin",
                        loginLink: "http://localhost:5170/auth/login",
                        supportEmail: "support@abb.com"
                    ),

                    "reset" or "password" => EmailTemplateHelper.GetPasswordResetEmail(
                        userName: "John Doe",
                        resetLink: "http://localhost:5170/auth/reset-password?token=abc123xyz",
                        expirationTime: "24 hours",
                        securityEmail: "security@abb.com"
                    ),

                    "notification" => EmailTemplateHelper.GetNotificationEmail(
                        title: "New Document Uploaded",
                        message: "A new document has been uploaded to your account and requires your attention. Please review it at your earliest convenience.",
                        referenceId: "DOC-2026-789",
                        type: "Document",
                        date: DateTime.Now.ToString("MMMM dd, yyyy HH:mm"),
                        actionLink: "http://localhost:5170/documents/789",
                        actionButtonText: "View Document",
                        iconBackgroundColor: "#0d6efd",
                        icon: "📎"
                    ),

                    "approval" => EmailTemplateHelper.GetApprovalRequiredEmail(
                        approverName: "Manager Name",
                        itemType: "Invoice",
                        itemNumber: "INV-2026-001",
                        amount: "Rp 50,000,000",
                        submittedBy: "John Doe",
                        submittedDate: "January 26, 2026 10:30 AM",
                        priority: "High",
                        notes: "Urgent payment required for vendor. This invoice is for critical services that need to be paid within 3 days to avoid service interruption.",
                        approveLink: "http://localhost:5170/approval/123/approve",
                        rejectLink: "http://localhost:5170/approval/123/reject",
                        viewDetailsLink: "http://localhost:5170/invoice/123"
                    ),

                    _ => $@"
                        <html>
                        <body style='font-family: Arial, sans-serif; padding: 40px;'>
                            <h1>Email Template Preview</h1>
                            <p>Available templates:</p>
                            <ul>
                                <li><a href='/api/test/email/preview/invoice'>Invoice Created</a></li>
                                <li><a href='/api/test/email/preview/contract'>Contract Created</a></li>
                                <li><a href='/api/test/email/preview/welcome'>Welcome Email</a></li>
                                <li><a href='/api/test/email/preview/reset'>Password Reset</a></li>
                                <li><a href='/api/test/email/preview/notification'>Notification</a></li>
                                <li><a href='/api/test/email/preview/approval'>Approval Required</a></li>
                            </ul>
                            <p style='color: #dc3545;'>Unknown template: {template}</p>
                        </body>
                        </html>
                    "
                };

                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                return Content($@"
                    <html>
                    <body style='font-family: Arial, sans-serif; padding: 40px;'>
                        <h1 style='color: #dc3545;'>Error Loading Template</h1>
                        <p><strong>Message:</strong> {ex.Message}</p>
                        <p><strong>Stack Trace:</strong></p>
                        <pre style='background: #f8f9fa; padding: 15px; border-radius: 4px;'>{ex.StackTrace}</pre>
                    </body>
                    </html>
                ", "text/html");
            }
        }

        /// <summary>
        /// List all available templates
        /// GET api/test/email/templates
        /// </summary>
        [HttpGet("templates")]
        public IActionResult ListTemplates()
        {
            var templates = new[]
            {
                new { name = "invoice", description = "Invoice Created Email", url = "/api/test/email/preview/invoice" },
                new { name = "contract", description = "Contract Created Email", url = "/api/test/email/preview/contract" },
                new { name = "welcome", description = "Welcome New User Email", url = "/api/test/email/preview/welcome" },
                new { name = "reset", description = "Password Reset Email", url = "/api/test/email/preview/reset" },
                new { name = "notification", description = "General Notification Email", url = "/api/test/email/preview/notification" },
                new { name = "approval", description = "Approval Required Email", url = "/api/test/email/preview/approval" }
            };

            return Ok(new
            {
                success = true,
                message = "Available email templates",
                data = templates,
                totalTemplates = templates.Length
            });
        }
    }
}
