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
            IsDeleted = x.IsDeleted
        };

    public static ContractDto ToDto(this Contract x)
    {
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
            IsDeleted = x.IsDeleted
        };
    }
}
