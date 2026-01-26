using System;
using System.Linq.Expressions;
using MonitoringDokumenGS.Dtos.Transaction;
using MonitoringDokumenGS.Models;

namespace MonitoringDokumenGS.Mappings.Transaction;

public static class AttachmentMappings
{
    public static readonly Expression<Func<Attachment, AttachmentDto>> ToDtoExpr =
        x => new AttachmentDto
        {
            AttachmentId = x.AttachmentId,
            AttachmentTypeId = x.AttachmentTypeId,
            ReferenceId = x.ReferenceId,
            FileName = x.FileName,
            FilePath = x.FilePath,
            FileSize = x.FileSize,
            CreatedAt = x.CreatedAt,
            CreatedBy = x.CreatedBy
        };

    public static AttachmentDto ToDto(this Attachment x)
    {
        return new AttachmentDto
        {
            AttachmentId = x.AttachmentId,
            AttachmentTypeId = x.AttachmentTypeId,
            ReferenceId = x.ReferenceId,
            FileName = x.FileName,
            FilePath = x.FilePath,
            FileSize = x.FileSize,
            CreatedAt = x.CreatedAt,
            CreatedBy = x.CreatedBy
        };
    }
}
