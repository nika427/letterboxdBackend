using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication38.Data;
using WebApplication38.DTO;

namespace WebApplication38.Controllers;

[ApiController]
[Route("api/follows")]
public class FollowController : ControllerBase
{
    private readonly DataContext _context;

    public FollowController(DataContext context)
    {
        _context = context;
    }

    [HttpGet("followers/{userId}")]
    public IActionResult GetFollowers(int userId)
    {
        var followers = _context.UserFollows
            .Include(x => x.Follower)
            .Where(x => x.FollowingId == userId)
            .Select(x => new UserDTO
            {
                Id = x.Follower!.Id,
                Username = x.Follower.Username,
                Email = x.Follower.Email,
                Role = x.Follower.Role
            })
            .ToList();

        return Ok(followers);
    }

    [HttpGet("following/{userId}")]
    public IActionResult GetFollowing(int userId)
    {
        var following = _context.UserFollows
            .Include(x => x.Following)
            .Where(x => x.FollowerId == userId)
            .Select(x => new UserDTO
            {
                Id = x.Following!.Id,
                Username = x.Following.Username,
                Email = x.Following.Email,
                Role = x.Following.Role
            })
            .ToList();

        return Ok(following);
    }

    [HttpPost("{userId}")]
    [Authorize]
    public IActionResult FollowUser(int userId)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();
        var currentUserId = int.Parse(userIdClaim.Value);

        if (currentUserId == userId)
        {
            return BadRequest("You cannot follow yourself.");
        }

        var userToFollow = _context.Users.FirstOrDefault(x => x.Id == userId);
        if (userToFollow == null)
        {
            return NotFound("User not found.");
        }

        var existing = _context.UserFollows.FirstOrDefault(x => x.FollowerId == currentUserId && x.FollowingId == userId);
        if (existing != null)
        {
            return BadRequest("Already following this user.");
        }

        var follow = new Models.UserFollow
        {
            FollowerId = currentUserId,
            FollowingId = userId
        };

        _context.UserFollows.Add(follow);
        _context.SaveChanges();

        return Ok("Now following user.");
    }

    [HttpDelete("{userId}")]
    [Authorize]
    public IActionResult UnfollowUser(int userId)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();
        var currentUserId = int.Parse(userIdClaim.Value);

        var follow = _context.UserFollows.FirstOrDefault(x => x.FollowerId == currentUserId && x.FollowingId == userId);
        if (follow == null)
        {
            return NotFound("Not following this user.");
        }

        _context.UserFollows.Remove(follow);
        _context.SaveChanges();

        return Ok("Unfollowed user.");
    }

    [HttpGet("is-following/{userId}")]
    public IActionResult IsFollowing(int userId)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Ok(false);
        var currentUserId = int.Parse(userIdClaim.Value);

        var isFollowing = _context.UserFollows.Any(x => x.FollowerId == currentUserId && x.FollowingId == userId);
        return Ok(isFollowing);
    }

    [HttpGet("count/followers/{userId}")]
    public IActionResult FollowersCount(int userId)
    {
        return Ok(_context.UserFollows.Count(x => x.FollowingId == userId));
    }

    [HttpGet("count/following/{userId}")]
    public IActionResult FollowingCount(int userId)
    {
        return Ok(_context.UserFollows.Count(x => x.FollowerId == userId));
    }

    [HttpGet("is-mutual/{userId}")]
    public IActionResult IsMutual(int userId)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Ok(false);
        var currentUserId = int.Parse(userIdClaim.Value);

        if (currentUserId == userId) return Ok(false);

        var iFollowThem = _context.UserFollows.Any(x => x.FollowerId == currentUserId && x.FollowingId == userId);
        var theyFollowMe = _context.UserFollows.Any(x => x.FollowerId == userId && x.FollowingId == currentUserId);

        return Ok(iFollowThem && theyFollowMe);
    }
}