using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication38.Core;
using WebApplication38.Data;
using WebApplication38.DTO;
using WebApplication38.Enums;
using WebApplication38.Models;
using WebApplication38.Requests;
using WebApplication38.Services;

namespace WebApplication38.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private readonly IJwtService _jwtService;
    private readonly EmailService _emailService;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public UserController(DataContext context, IMapper mapper, IJwtService jwtService, EmailService emailService)
    {
        _context = context;
        _mapper = mapper;
        _jwtService = jwtService;
        _emailService = emailService;
    }

    [HttpGet("get-all")]
    [Authorize]
    public IActionResult GetAllUsers()
    {
        var users = _context.Users.ToList();
        var result = _mapper.Map<List<UserDTO>>(users);
        return Ok(result);
    }

    [HttpGet("get-by-id/{id}")]
    public IActionResult GetUserById(int id)
    {
        var user = _context.Users.FirstOrDefault(x => x.Id == id);
        if (user == null) return NotFound();

        return Ok(_mapper.Map<UserDTO>(user));
    }

    [HttpPost("register")]
    public IActionResult Register(AddUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("All fields are required.");
        }

        var existingUser = _context.Users
            .FirstOrDefault(x => x.Email == request.Email || x.Username == request.Username);

        if (existingUser != null)
        {
            return BadRequest("Username or email already exists.");
        }

        var random = new Random();
        var code = random.Next(100000, 999999).ToString();

        var user = _mapper.Map<User>(request);

        user.Role = ROLES.User;
        user.EmailVerified = false;
        user.VerificationCode = code;
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _context.Users.Add(user);
        _context.SaveChanges();

        try
        {
            _ = _emailService.SendVerificationEmailAsync(user.Email, code);
        }
        catch
        {
            // Email sending failed, but user is still registered
        }

        return Ok("User registered. Check your email for verification code.");
    }

    [HttpPost("verify-email")]
    public IActionResult VerifyEmail(string email, string code)
    {
        var user = _context.Users.FirstOrDefault(x => x.Email == email);

        if (user == null)
            return NotFound("User not found.");

        if (user.VerificationCode != code)
            return BadRequest("Invalid code.");

        user.EmailVerified = true;
        user.VerificationCode = null;

        _context.SaveChanges();

        return Ok("Email verified successfully.");
    }

    [HttpPost("login")]
    public IActionResult Login(LoginRequest request)
    {
        var user = _context.Users.FirstOrDefault(x => x.Email == request.Email);

        if (user == null)
            return Unauthorized("Invalid email or password.");

        var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (verify == PasswordVerificationResult.Failed)
            return Unauthorized("Invalid email or password.");

        if (!user.EmailVerified)
            return Unauthorized("Please verify your email first.");

        var token = _jwtService.CreateToken(user);

        var response = new UserToken
        {
            Token = token,
            User = _mapper.Map<UserDTO>(user),
            Role = user.Role
        };

        return Ok(response);
    }

    [HttpPut("update/{id}")]
    [Authorize]
    public IActionResult UpdateUser(int id, UpdateUserRequest request)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();
        var currentUserId = int.Parse(userIdClaim.Value);

        var user = _context.Users.FirstOrDefault(x => x.Id == id);
        if (user == null) return NotFound();

        if (currentUserId != id)
        {
            return Forbid("You can only update your own profile.");
        }

        var duplicate = _context.Users
            .FirstOrDefault(x => x.Id != id &&
                (x.Username == request.Username || x.Email == request.Email));

        if (duplicate != null)
            return BadRequest("Username or email already exists.");

        user.Username = request.Username;
        user.Email = request.Email;

        _context.SaveChanges();

        return Ok(_mapper.Map<UserDTO>(user));
    }

    [HttpDelete("delete/{id}")]
    [Authorize]
    public IActionResult DeleteUser(int id)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();
        var currentUserId = int.Parse(userIdClaim.Value);

        var user = _context.Users.FirstOrDefault(x => x.Id == id);
        if (user == null) return NotFound();

        if (currentUserId == id)
        {
            return BadRequest("You cannot delete your own account.");
        }

        _context.Users.Remove(user);
        _context.SaveChanges();

        return Ok("User deleted successfully.");
    }

    [HttpPut("update-role/{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult UpdateRole(int id, [FromBody] UpdateRoleRequest request)
    {
        var user = _context.Users.FirstOrDefault(x => x.Id == id);
        if (user == null) return NotFound();

        user.Role = (ROLES)request.role;
        _context.SaveChanges();

        return Ok("Role updated.");
    }

    [HttpGet("search-by-username")]
    [Authorize]
    public IActionResult SearchByUsername(string username)
    {
        var users = _context.Users.Where(x => x.Username.Contains(username)).ToList();
        return Ok(_mapper.Map<List<UserDTO>>(users));
    }

    [HttpGet("search-by-email")]
    [Authorize]
    public IActionResult SearchByEmail(string email)
    {
        var users = _context.Users.Where(x => x.Email.Contains(email)).ToList();
        return Ok(_mapper.Map<List<UserDTO>>(users));
    }

    [HttpGet("count")]
    [Authorize(Roles = "Admin")]
    public IActionResult CountUsers()
    {
        return Ok(_context.Users.Count());
    }
}