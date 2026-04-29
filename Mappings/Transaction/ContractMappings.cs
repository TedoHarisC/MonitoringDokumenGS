using System;
using System.Linq.Expressions;
using MonitoringDokumenGS.Dtos.Transaction;
using MonitoringDokumenGS.Models;

namespace MonitoringDokumenGS.Mappings.Transaction;

public static class ContractMappings
{
    public static readonly Expression<Func<Contract, ContractDto>> ToDtoExpr =
        x => new ContractDto
        {
            ContractId = x.ContractId,
            VendorId = x.VendorId,
            VendorName = x.Vendor.VendorName,
            CreatedByUserId = x.CreatedByUserId,
            ContractNumber = x.ContractNumber,
            ContractDescription = x.ContractDescription,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            ApprovalStatusId = x.ApprovalStatusId,
            ContractStatusId = x.ContractStatusId,
            CreatedAt = x.CreatedAt,
            CreatedBy = x.CreatedBy,
            UpdatedAt = x.UpdatedAt,
            UpdatedBy = x.UpdatedBy,
            IsDeleted = x.IsDeleted,
            DaysUntilExpiry = (int)(x.EndDate.Date - DateTime.Today).TotalDays,
            ValidityStatus = x.EndDate.Date < DateTime.Today ? "Expired" :
                           (x.EndDate.Date - DateTime.Today).TotalDays <= 30 ? "Expiring Soon" : "Active"
        };

    public static ContractDto ToDto(this Contract x)
    {
        var daysUntilExpiry = (int)(x.EndDate.Date - DateTime.Today).TotalDays;
        var validityStatus = x.EndDate.Date < DateTime.Today ? "Expired" :
                           daysUntilExpiry <= 30 ? "Expiring Soon" : "Active";

        return new ContractDto
        {
            ContractId = x.ContractId,
            VendorId = x.VendorId,
            CreatedByUserId = x.CreatedByUserId,
            ContractNumber = x.ContractNumber,
            ContractDescription = x.ContractDescription,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            ApprovalStatusId = x.ApprovalStatusId,
            ContractStatusId = x.ContractStatusId,
            CreatedAt = x.CreatedAt,
            CreatedBy = x.CreatedBy,
            UpdatedAt = x.UpdatedAt,
            UpdatedBy = x.UpdatedBy,
            IsDeleted = x.IsDeleted,
            DaysUntilExpiry = daysUntilExpiry,
            ValidityStatus = validityStatus
        };
    }
}
