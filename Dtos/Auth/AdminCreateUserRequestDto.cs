namespace MonitoringDokumenGS.Dtos.Auth
{
    public class AdminCreateUserRequestDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Guid? VendorId { get; set; }
        public bool IsActive { get; set; } = true;
        public int RoleId { get; set; }
    }
}
