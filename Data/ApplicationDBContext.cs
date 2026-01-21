

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

        // Define other DbSets for your entities here
    }
}