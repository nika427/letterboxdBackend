using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication38.Data;
using WebApplication38.DTO;

namespace WebApplication38.Controllers;

[ApiController]
[Route("api/activity")]
public class ActivityController : ControllerBase
{
    private readonly DataContext _context;

    public ActivityController(DataContext context)
    {
        _context = context;
    }

    [HttpGet("{userId}")]
    public IActionResult GetUserActivity(int userId)
    {
        var reviewItems = _context.Reviews
            .Include(x => x.Movie)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(10)
            .ToList()
            .Select(x => new ActivityItemDto
            {
                Type = "review",
                Id = x.Id,
                MovieId = x.MovieId,
                MovieTitle = x.Movie != null ? x.Movie.Title : "",
                MoviePoster = x.Movie?.PosterUrl ?? "",
                Rating = x.Rating,
                Comment = x.Comment,
                CreatedAt = x.CreatedAt
            })
            .ToList();

        var wishlistItems = _context.Wishlists
            .Include(x => x.Movie)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(10)
            .ToList()
            .Select(x => new ActivityItemDto
            {
                Type = "wishlist",
                Id = x.Id,
                MovieId = x.MovieId,
                MovieTitle = x.Movie != null ? x.Movie.Title : "",
                MoviePoster = x.Movie?.PosterUrl ?? "",
                CreatedAt = x.CreatedAt
            })
            .ToList();

        var activity = reviewItems
            .Concat(wishlistItems)
            .OrderByDescending(x => x.CreatedAt)
            .Take(20)
            .ToList();

        return Ok(activity);
    }

    [HttpGet("feed")]
    [Authorize]
    public IActionResult GetFeed()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();
        var currentUserId = int.Parse(userIdClaim.Value);

        var followingIds = _context.UserFollows
            .Where(x => x.FollowerId == currentUserId)
            .Select(x => x.FollowingId)
            .ToList();

        if (!followingIds.Any())
        {
            return Ok(new List<FeedItemDto>());
        }

        var reviews = _context.Reviews
            .Include(x => x.Movie)
            .Include(x => x.User)
            .Where(x => followingIds.Contains(x.UserId))
            .OrderByDescending(x => x.CreatedAt)
            .Take(20)
            .ToList()
            .Select(x => new FeedItemDto
            {
                Type = "review",
                Id = x.Id,
                UserId = x.UserId,
                Username = x.User != null ? x.User.Username : "",
                MovieId = x.MovieId,
                MovieTitle = x.Movie != null ? x.Movie.Title : "",
                MoviePoster = x.Movie?.PosterUrl ?? "",
                Rating = x.Rating,
                Comment = x.Comment,
                CreatedAt = x.CreatedAt
            })
            .ToList();

        return Ok(reviews);
    }
}