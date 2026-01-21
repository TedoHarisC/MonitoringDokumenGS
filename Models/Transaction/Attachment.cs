using System.ComponentModel.DataAnnotations;

namespace MonitoringDokumenGS.Models
{
    public class Attachment
    {
        [Key]
        public Guid AttachmentId { get; set; }
        public int AttachmentTypeId { get; set; }
        public Guid ReferenceId { get; set; }
        public string FileName { get; set; } = default!;
        public string FilePath { get; set; } = default!;
        public int FileSize { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }

        //Navigation properties
        public AttachmentTypes AttachmentType { get; set; } = default!;
        public UserModel Creator { get; set; } = default!;
    }
}