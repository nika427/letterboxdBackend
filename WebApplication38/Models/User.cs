using WebApplication38.Enums;

namespace WebApplication38.Models;

public class User
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public ROLES Role { get; set; } = ROLES.User;

    public bool EmailVerified { get; set; }

    public string? VerificationCode { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public List<Review> Reviews { get; set; } = new();
    public List<Wishlist> Wishlists { get; set; } = new();
    public List<UserFollow> Followers { get; set; } = new();
    public List<UserFollow> Following { get; set; } = new();
}
