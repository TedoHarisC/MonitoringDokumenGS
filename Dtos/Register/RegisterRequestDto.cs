namespace Dtos.Register
{
    public class RegisterRequestDto
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string Email { get; set; }
        // Optional vendor id for multi-tenant scenarios
        public Guid? VendorId { get; set; }
    }
}