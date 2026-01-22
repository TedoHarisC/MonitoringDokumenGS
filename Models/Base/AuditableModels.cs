public abstract class AuditableEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? CreatedBy { get; set; } = Guid.Empty;

    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}
