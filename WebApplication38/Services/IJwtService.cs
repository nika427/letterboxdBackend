using WebApplication38.Models;

namespace WebApplication38.Services;

public interface IJwtService
{
    string CreateToken(User user);
}
