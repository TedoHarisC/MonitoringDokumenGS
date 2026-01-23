

using Microsoft.EntityFrameworkCore;
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
            modelBuilder.Entity<UserRoles>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });
        }

    }
}