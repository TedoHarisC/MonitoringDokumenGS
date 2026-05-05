using System;
using System.Linq.Expressions;
using MonitoringDokumenGS.Dtos.Transaction;
using MonitoringDokumenGS.Models;

namespace MonitoringDokumenGS.Mappings.Transaction;

public static class InvoiceMappings
{
    public static readonly Expression<Func<Invoice, InvoiceDto>> ToDtoExpr =
        x => new InvoiceDto
        {
            InvoiceId = x.InvoiceId,
            VendorId = x.VendorId,
            CreatedByUserId = x.CreatedByUserId,
            InvoiceNumber = x.InvoiceNumber,
            InvoiceDescription = x.InvoiceDescription,
            ProgressStatusId = x.ProgressStatusId,
            ProgressStatusName = x.ProgressStatus != null ? x.ProgressStatus.Name : null,
            VendorName = x.Vendor != null ? x.Vendor.VendorName : null,
            InvoiceAmount = x.InvoiceAmount,
            TaxAmount = x.TaxAmount,
            CreatedAt = x.CreatedAt,
            CreatedBy = x.CreatedBy,
            UpdatedAt = x.UpdatedAt,
            UpdatedBy = x.UpdatedBy,
            IsDeleted = x.IsDeleted,
            InvoiceYear = x.InvoiceYear,
            InvoiceMonth = x.InvoiceMonth,
            NoSAP = x.NoSAP,
            GrandTotal = x.GrandTotal ?? 0,
            BudgetCodeId = x.BudgetCodeId,
            BudgetCode = x.Budget != null && x.Budget.BudgetCode != null ? x.Budget.BudgetCode.Description : null,
            CoaTextId = x.CoaTextId,
            CoaText = x.Coa != null ? x.Coa.Name : null,
            // Calculate IsOnTime: CreatedAt should be <= 7th day of InvoiceYear/InvoiceMonth
            IsOnTime = x.CreatedAt.Date <= new DateTime(x.InvoiceYear, x.InvoiceMonth, 7).Date
        };

    public static InvoiceDto ToDto(this Invoice x)
    {
        return new InvoiceDto
        {
            InvoiceId = x.InvoiceId,
            VendorId = x.VendorId,
            CreatedByUserId = x.CreatedByUserId,
            InvoiceNumber = x.InvoiceNumber,
            InvoiceDescription = x.InvoiceDescription,
            ProgressStatusId = x.ProgressStatusId,
            ProgressStatusName = x.ProgressStatus != null ? x.ProgressStatus.Name : null,
            VendorName = x.Vendor != null ? x.Vendor.VendorName : null,
            InvoiceAmount = x.InvoiceAmount,
            TaxAmount = x.TaxAmount,
            CreatedAt = x.CreatedAt,
            CreatedBy = x.CreatedBy,
            UpdatedAt = x.UpdatedAt,
            UpdatedBy = x.UpdatedBy,
            IsDeleted = x.IsDeleted,
            InvoiceYear = x.InvoiceYear,
            InvoiceMonth = x.InvoiceMonth,
            NoSAP = x.NoSAP,
            GrandTotal = x.GrandTotal ?? 0,
            BudgetCodeId = x.BudgetCodeId,
            CoaTextId = x.CoaTextId,
            // Calculate IsOnTime: CreatedAt should be <= 7th day of InvoiceYear/InvoiceMonth
            IsOnTime = x.CreatedAt.Date <= new DateTime(x.InvoiceYear, x.InvoiceMonth, 7).Date
        };
    }
}
