using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApplication38.Core;
using WebApplication38.Enums;
using WebApplication38.Models;

namespace WebApplication38.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string CreateToken(User user)
    {
        var key = _configuration["Jwt:Key"] ?? "ThisIsAVeryLongDefaultJwtSecretKeyForDevelopmentOnly123!";
        var issuer = _configuration["Jwt:Issuer"] ?? "http://localhost:5001";
        var audience = _configuration["Jwt:Audience"] ?? "http://localhost:4200";

        var roleName = user.Role == ROLES.Admin ? "Admin" : "User";

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, roleName)
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}


