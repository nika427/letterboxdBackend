using WebApplication38.DTO;
using WebApplication38.Enums;

namespace WebApplication38.Core;

public class UserToken
{
    public string Token { get; set; } = string.Empty;
    public UserDTO? User { get; set; }
    public ROLES Role { get; set; }
}
