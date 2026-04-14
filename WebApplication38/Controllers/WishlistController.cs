using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication38.Data;
using WebApplication38.DTO;

namespace WebApplication38.Controllers;

[ApiController]
[Route("api/wishlists")]
public class WishlistController : ControllerBase
{
    private readonly DataContext _context;

    public WishlistController(DataContext context)
    {
        _context = context;
    }

    [HttpGet("{userId}")]
    [Authorize]
    public IActionResult GetUserWishlist(int userId)
    {
        try
        {
            var wishlist = _context.Wishlists
                .Include(x => x.Movie)
                .Where(x => x.UserId == userId)
                .Select(x => new MovieDTO
                {
                    Id = x.Movie!.Id,
                    Title = x.Movie.Title,
                    Description = x.Movie.Description,
                    ReleaseYear = x.Movie.ReleaseYear,
                    Genre = x.Movie.Genre,
                    PosterUrl = x.Movie.PosterUrl,
                    AverageRating = 0,
                    ReviewCount = 0,
                    ActorCount = 0
                })
                .ToList();

            return Ok(wishlist);
        }
        catch
        {
            return Ok(new List<MovieDTO>());
        }
    }

    [HttpPost]
    [Authorize]
    public IActionResult AddToWishlist(int movieId)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();
        var userId = int.Parse(userIdClaim.Value);

        var existing = _context.Wishlists.FirstOrDefault(x => x.UserId == userId && x.MovieId == movieId);
        if (existing != null)
        {
            return BadRequest("Movie already in wishlist.");
        }

        var movie = _context.Movies.FirstOrDefault(x => x.Id == movieId);
        if (movie == null)
        {
            return NotFound("Movie not found.");
        }

        var wishlist = new Models.Wishlist
        {
            UserId = userId,
            MovieId = movieId
        };

        _context.Wishlists.Add(wishlist);
        _context.SaveChanges();

        return Ok("Added to wishlist.");
    }

    [HttpDelete("{id}")]
    [Authorize]
    public IActionResult RemoveFromWishlist(int id)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();
        var userId = int.Parse(userIdClaim.Value);

        var wishlist = _context.Wishlists.FirstOrDefault(x => x.Id == id && x.UserId == userId);
        if (wishlist == null)
        {
            return NotFound();
        }

        _context.Wishlists.Remove(wishlist);
        _context.SaveChanges();

        return Ok("Removed from wishlist.");
    }

    [HttpGet("check/{movieId}")]
    [Authorize]
    public IActionResult CheckInWishlist(int movieId)
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim.Value);

            var exists = _context.Wishlists.Any(x => x.UserId == userId && x.MovieId == movieId);
            return Ok(exists);
        }
        catch
        {
            return Ok(false);
        }
    }

    [HttpGet("count/{userId}")]
    public IActionResult WishlistCount(int userId)
    {
        return Ok(_context.Wishlists.Count(x => x.UserId == userId));
    }
}