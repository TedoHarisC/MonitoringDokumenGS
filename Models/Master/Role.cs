using System.ComponentModel.DataAnnotations;

namespace MonitoringDokumenGS.Models
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}