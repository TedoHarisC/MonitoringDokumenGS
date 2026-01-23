using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MonitoringDokumenGS.Models;

namespace MonitoringDokumenGS.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<AttachmentTypes> AttachmentTypes { get; set; }
        public DbSet<InvoiceProgressStatuses> InvoiceProgressStatuses { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Notifications> Notifications { get; set; }
        public DbSet<VendorCategory> VendorCategories { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<ApprovalStatus> ApprovalStatuses { get; set; }
        public DbSet<ContractStatus> ContractStatuses { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRoles> UserRoles { get; set; }

        // Define other DbSets for your entities here

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Fix InvalidCastException: some databases store this flag as BIGINT (0/1).
            // Apply narrowly so we don't break entities whose columns are normal SQL Server BIT.
            var boolToLongConverter = new ValueConverter<bool, long>(v => v ? 1L : 0L, v => v == 1L);

            modelBuilder.Entity<UserRoles>()
                .Property(x => x.IsDeleted)
                .HasConversion(boolToLongConverter);

            modelBuilder.Entity<UserRoles>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRoles>()
                .HasOne<UserModel>()
                .WithMany()
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRoles>()
                .HasOne<Role>()
                .WithMany()
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}