namespace MonitoringDokumenGS.Models
{
    public class UserRoles : SoftDeletableEntity
    {
        public Guid UserId { get; set; }
        public int RoleId { get; set; }
    }
}