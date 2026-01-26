using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace MonitoringDokumenGS.Services.Infrastructure
{
    /// <summary>
    /// Email template helper untuk load dan populate email templates
    /// </summary>
    public static class EmailTemplateHelper
    {
        private static readonly string TemplateBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmailTemplates");

        /// <summary>
        /// Load template dari file dan replace placeholders dengan data
        /// </summary>
        public static string LoadTemplate(string templateName, Dictionary<string, string> data)
        {
            var templatePath = Path.Combine(TemplateBasePath, templateName);

            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Email template not found: {templateName}", templatePath);
            }

            var html = File.ReadAllText(templatePath);
            return ReplacePlaceholders(html, data);
        }

        /// <summary>
        /// Replace all placeholders {{Key}} dengan values dari dictionary
        /// </summary>
        private static string ReplacePlaceholders(string html, Dictionary<string, string> data)
        {
            foreach (var item in data)
            {
                html = html.Replace($"{{{{{item.Key}}}}}", item.Value ?? string.Empty);
            }
            return html;
        }

        #region Notification Email

        public static string GetNotificationEmail(
            string title,
            string message,
            string referenceId,
            string type,
            string date,
            string actionLink,
            string actionButtonText = "View Details",
            string iconBackgroundColor = "#0d6efd",
            string icon = "🔔")
        {
            var data = new Dictionary<string, string>
            {
                { "NotificationTitle", title },
                { "NotificationMessage", message },
                { "ReferenceId", referenceId },
                { "Type", type },
                { "Date", date },
                { "ActionLink", actionLink },
                { "ActionButtonText", actionButtonText },
                { "IconBackgroundColor", iconBackgroundColor },
                { "Icon", icon }
            };

            return LoadTemplate("Notification.html", data);
        }

        #endregion

        #region Invoice Email

        public static string GetInvoiceCreatedEmail(
            string invoiceNumber,
            string vendorName,
            string amount,
            string dueDate,
            string status,
            string createdBy,
            string createdDate,
            string description,
            string invoiceLink)
        {
            var data = new Dictionary<string, string>
            {
                { "InvoiceNumber", invoiceNumber },
                { "VendorName", vendorName },
                { "Amount", amount },
                { "DueDate", dueDate },
                { "Status", status },
                { "CreatedBy", createdBy },
                { "CreatedDate", createdDate },
                { "Description", description },
                { "InvoiceLink", invoiceLink }
            };

            return LoadTemplate("InvoiceCreated.html", data);
        }

        #endregion

        #region Contract Email

        public static string GetContractCreatedEmail(
            string contractNumber,
            string vendorName,
            string contractValue,
            string startDate,
            string endDate,
            string status,
            string createdBy,
            string createdDate,
            string description,
            string contractLink)
        {
            var data = new Dictionary<string, string>
            {
                { "ContractNumber", contractNumber },
                { "VendorName", vendorName },
                { "ContractValue", contractValue },
                { "StartDate", startDate },
                { "EndDate", endDate },
                { "Status", status },
                { "CreatedBy", createdBy },
                { "CreatedDate", createdDate },
                { "Description", description },
                { "ContractLink", contractLink }
            };

            return LoadTemplate("ContractCreated.html", data);
        }

        #endregion

        #region Welcome Email

        public static string GetWelcomeEmail(
            string userName,
            string username,
            string email,
            string role,
            string loginLink,
            string supportEmail = "support@abb.com")
        {
            var data = new Dictionary<string, string>
            {
                { "UserName", userName },
                { "Username", username },
                { "Email", email },
                { "Role", role },
                { "LoginLink", loginLink },
                { "SupportEmail", supportEmail }
            };

            return LoadTemplate("Welcome.html", data);
        }

        #endregion

        #region Password Reset Email

        public static string GetPasswordResetEmail(
            string userName,
            string resetLink,
            string expirationTime = "24 hours",
            string securityEmail = "security@abb.com")
        {
            var data = new Dictionary<string, string>
            {
                { "UserName", userName },
                { "ResetLink", resetLink },
                { "ExpirationTime", expirationTime },
                { "SecurityEmail", securityEmail }
            };

            return LoadTemplate("PasswordReset.html", data);
        }

        #endregion

        #region Approval Email

        public static string GetApprovalRequiredEmail(
            string approverName,
            string itemType,
            string itemNumber,
            string amount,
            string submittedBy,
            string submittedDate,
            string priority,
            string notes,
            string approveLink,
            string rejectLink,
            string viewDetailsLink)
        {
            var data = new Dictionary<string, string>
            {
                { "ApproverName", approverName },
                { "ItemType", itemType },
                { "ItemNumber", itemNumber },
                { "Amount", amount },
                { "SubmittedBy", submittedBy },
                { "SubmittedDate", submittedDate },
                { "Priority", priority },
                { "Notes", notes },
                { "ApproveLink", approveLink },
                { "RejectLink", rejectLink },
                { "ViewDetailsLink", viewDetailsLink }
            };

            return LoadTemplate("ApprovalRequired.html", data);
        }

        #endregion

        #region Base Layout Email (for custom content)

        public static string GetBaseLayoutEmail(
            string emailTitle,
            string companyName,
            string emailContent,
            string year,
            string companyAddress,
            string unsubscribeLink = "#",
            string privacyLink = "#",
            string helpLink = "#")
        {
            var data = new Dictionary<string, string>
            {
                { "EmailTitle", emailTitle },
                { "CompanyName", companyName },
                { "EmailContent", emailContent },
                { "Year", year },
                { "CompanyAddress", companyAddress },
                { "UnsubscribeLink", unsubscribeLink },
                { "PrivacyLink", privacyLink },
                { "HelpLink", helpLink }
            };

            return LoadTemplate("BaseLayout.html", data);
        }

        #endregion
    }
}
