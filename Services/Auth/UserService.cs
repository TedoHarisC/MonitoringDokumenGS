using Microsoft.EntityFrameworkCore;
using MonitoringDokumenGS.Data;
using MonitoringDokumenGS.Dtos.Auth;
using MonitoringDokumenGS.Interfaces;
using MonitoringDokumenGS.Models;
using BCryptNet = BCrypt.Net.BCrypt;

namespace MonitoringDokumenGS.Services
{
    public class UserService : IUser
    {
        private readonly ApplicationDBContext _context;
        private readonly IAuditLog _auditLog;

        public UserService(ApplicationDBContext context, IAuditLog auditLog)
        {
            _context = context;
            _auditLog = auditLog;
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .Include(u => u.Vendor)
                .Where(u => !u.isDeleted)
                .OrderBy(u => u.Username)
                .Select(u => new UserDto
                {
                    UserId = u.UserId,
                    VendorId = u.VendorId,
                    Username = u.Username,
                    Email = u.Email,
                    IsActive = u.isActive,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    IsDeleted = u.isDeleted,
                    LastLoginAt = u.LastLoginAt,
                    VendorName = u.Vendor != null ? u.Vendor.VendorName : null
                })
                .ToListAsync();
        }

        public async Task<UserDto?> GetByIdAsync(Guid id)
        {
            return await _context.Users
                .AsNoTracking()
                .Include(u => u.Vendor)
                .Where(u => u.UserId == id && !u.isDeleted)
                .Select(u => new UserDto
                {
                    UserId = u.UserId,
                    VendorId = u.VendorId,
                    Username = u.Username,
                    Email = u.Email,
                    IsActive = u.isActive,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    IsDeleted = u.isDeleted,
                    LastLoginAt = u.LastLoginAt,
                    VendorName = u.Vendor != null ? u.Vendor.VendorName : null
                })
                .FirstOrDefaultAsync();
        }

        public async Task<UserDto?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.Username == username && !u.isDeleted)
                .Select(u => new UserDto
                {
                    UserId = u.UserId,
                    VendorId = u.VendorId,
                    Username = u.Username,
                    Email = u.Email,
                    IsActive = u.isActive,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    IsDeleted = u.isDeleted,
                    LastLoginAt = u.LastLoginAt,
                    VendorName = u.Vendor != null ? u.Vendor.VendorName : null
                })
                .FirstOrDefaultAsync();
        }

        public async Task<UserDto> CreateAsync(UserDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Username))
                throw new ArgumentException("Username is required.", nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new ArgumentException("Email is required.", nameof(dto));

            // Check if username or email already exists (excluding soft-deleted users)
            var exists = await _context.Users
                .AnyAsync(u => (u.Username == dto.Username || u.Email == dto.Email) && !u.isDeleted);

            if (exists)
                throw new InvalidOperationException("Username or email already exists");

            if (dto.VendorId != Guid.Empty)
            {
                var vendorExists = await _context.Vendors.AnyAsync(v => v.VendorId == dto.VendorId);
                if (!vendorExists)
                    throw new InvalidOperationException("Vendor not found");
            }

            var newUserId = Guid.NewGuid();

            // NOTE: IUser.CreateAsync memakai UserDto (tanpa password). Untuk memenuhi kolom required,
            // kita set password random. User bisa melakukan reset password via AuthService.
            var entity = new UserModel
            {
                UserId = newUserId,
                VendorId = dto.VendorId,
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCryptNet.HashPassword(Guid.NewGuid().ToString("N")),
                isActive = dto.IsActive,
                isDeleted = dto.IsDeleted,
                CreatedBy = newUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LastLoginAt = dto.LastLoginAt,
            };

            _context.Users.Add(entity);
            await _context.SaveChangesAsync();

            var result = new UserDto
            {
                UserId = entity.UserId,
                VendorId = entity.VendorId,
                Username = entity.Username,
                Email = entity.Email,
                IsActive = entity.isActive,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                IsDeleted = entity.isDeleted,
                LastLoginAt = entity.LastLoginAt
            };

            await _auditLog.LogAsync("User", "Create", null, result, entity.UserId.ToString());
            return result;
        }

        public async Task<bool> UpdateAsync(UserDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));

            var entity = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == dto.UserId);

            if (entity == null)
                return false;

            if (dto.VendorId != Guid.Empty)
            {
                var vendorExists = await _context.Vendors.AnyAsync(v => v.VendorId == dto.VendorId);
                if (!vendorExists)
                    throw new InvalidOperationException("Vendor not found");
            }

            var old = new UserDto
            {
                UserId = entity.UserId,
                VendorId = entity.VendorId,
                Username = entity.Username,
                Email = entity.Email,
                IsActive = entity.isActive,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                IsDeleted = entity.isDeleted,
                LastLoginAt = entity.LastLoginAt
            };

            entity.VendorId = dto.VendorId;
            entity.Username = dto.Username;
            entity.Email = dto.Email;
            entity.isActive = dto.IsActive;
            entity.isDeleted = dto.IsDeleted;
            entity.LastLoginAt = dto.LastLoginAt;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var updated = new UserDto
            {
                UserId = entity.UserId,
                VendorId = entity.VendorId,
                Username = entity.Username,
                Email = entity.Email,
                IsActive = entity.isActive,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                IsDeleted = entity.isDeleted,
                LastLoginAt = entity.LastLoginAt
            };

            await _auditLog.LogAsync("User", "Update", old, updated, entity.UserId.ToString());
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (entity == null)
                return false;

            var old = new UserDto
            {
                UserId = entity.UserId,
                VendorId = entity.VendorId,
                Username = entity.Username,
                Email = entity.Email,
                IsActive = entity.isActive,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                IsDeleted = entity.isDeleted,
                LastLoginAt = entity.LastLoginAt
            };

            entity.Username = $"[DELETED_{entity.UserId}]_{entity.Username}"; //Digunakan untuk menghindari duplicate username di masa depan
            entity.Email = $"[DELETED_{entity.UserId}]_{entity.Email}";       //Digunakan untuk menghindari duplicate email di masa depan
            entity.isDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditLog.LogAsync("User", "Delete", old, null, id.ToString());
            return true;
        }
    }
}
