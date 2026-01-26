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
            ProgressStatusId = x.ProgressStatusId,
            InvoiceAmount = x.InvoiceAmount,
            TaxAmount = x.TaxAmount,
            CreatedAt = x.CreatedAt,
            CreatedBy = x.CreatedBy,
            UpdatedAt = x.UpdatedAt,
            UpdatedBy = x.UpdatedBy,
            IsDeleted = x.IsDeleted
        };

    public static InvoiceDto ToDto(this Invoice x)
    {
        return new InvoiceDto
        {
            InvoiceId = x.InvoiceId,
            VendorId = x.VendorId,
            CreatedByUserId = x.CreatedByUserId,
            InvoiceNumber = x.InvoiceNumber,
            ProgressStatusId = x.ProgressStatusId,
            InvoiceAmount = x.InvoiceAmount,
            TaxAmount = x.TaxAmount,
            CreatedAt = x.CreatedAt,
            CreatedBy = x.CreatedBy,
            UpdatedAt = x.UpdatedAt,
            UpdatedBy = x.UpdatedBy,
            IsDeleted = x.IsDeleted
        };
    }
}
