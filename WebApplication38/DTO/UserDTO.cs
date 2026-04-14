using WebApplication38.Enums;

namespace WebApplication38.DTO;

public class UserDTO
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public ROLES Role { get; set; }
    public bool EmailVerified { get; set; }
}
