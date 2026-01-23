namespace MonitoringDokumenGS.Dtos.Auth
{
    public class AdminUpdateUserRequestDto
    {
        public string Username { get; set; } = string.Empty;
        public string? Password { get; set; }
        public string Email { get; set; } = string.Empty;
        public Guid? VendorId { get; set; }
        public bool IsActive { get; set; } = true;
        public int RoleId { get; set; }
    }
}
