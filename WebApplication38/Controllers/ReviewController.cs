using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication38.Data;
using WebApplication38.DTO;
using WebApplication38.Models;
using WebApplication38.Requests;

namespace WebApplication38.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public ReviewController(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet("get-all")]
    public IActionResult GetAllReviews()
    {
        var reviews = _context.Reviews
            .Include(x => x.User)
            .Include(x => x.Movie)
            .ToList();

        var result = reviews.Select(x => new ReviewDTO
        {
            Id = x.Id,
            Rating = x.Rating,
            Comment = x.Comment,
            CreatedAt = x.CreatedAt,
            UserId = x.UserId,
            Username = x.User != null ? x.User.Username : string.Empty,
            MovieId = x.MovieId,
            MovieTitle = x.Movie != null ? x.Movie.Title : string.Empty
        }).ToList();

        return Ok(result);
    }

    [HttpGet("get-by-id/{id}")]
    public IActionResult GetReviewById(int id)
    {
        var review = _context.Reviews
            .Include(x => x.User)
            .Include(x => x.Movie)
            .FirstOrDefault(x => x.Id == id);

        if (review == null)
        {
            return NotFound();
        }

        var result = new ReviewDTO
        {
            Id = review.Id,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt,
            UserId = review.UserId,
            Username = review.User != null ? review.User.Username : string.Empty,
            MovieId = review.MovieId,
            MovieTitle = review.Movie != null ? review.Movie.Title : string.Empty
        };

        return Ok(result);
    }

    [HttpPost("add")]
    [Authorize]
    public IActionResult AddReview(AddReviewRequest request)
    {
        if (request.Rating < 1 || request.Rating > 10)
        {
            return BadRequest("Rating must be between 1 and 10.");
        }

        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();
        var userId = int.Parse(userIdClaim.Value);

        var user = _context.Users.FirstOrDefault(x => x.Id == userId);
        var movie = _context.Movies.FirstOrDefault(x => x.Id == request.MovieId);

        if (user == null || movie == null)
        {
            return NotFound("User or movie not found.");
        }

        var existingReview = _context.Reviews.FirstOrDefault(x => x.UserId == userId && x.MovieId == request.MovieId);
        if (existingReview != null)
        {
            return BadRequest("You already reviewed this movie.");
        }

        var review = new Review
        {
            Rating = request.Rating,
            Comment = request.Comment,
            MovieId = request.MovieId,
            UserId = userId,
            CreatedAt = DateTime.Now
        };

        _context.Reviews.Add(review);
        _context.SaveChanges();

        var result = new ReviewDTO
        {
            Id = review.Id,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt,
            UserId = review.UserId,
            Username = user.Username,
            MovieId = review.MovieId,
            MovieTitle = movie.Title
        };

        return Ok(result);
    }

    [HttpPut("update/{id}")]
    [Authorize]
    public IActionResult UpdateReview(int id, UpdateReviewRequest request)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();
        var currentUserId = int.Parse(userIdClaim.Value);

        var review = _context.Reviews.Include(x => x.User).Include(x => x.Movie).FirstOrDefault(x => x.Id == id);
        if (review == null)
        {
            return NotFound();
        }

        if (review.UserId != currentUserId)
        {
            return Forbid("You can only update your own reviews.");
        }

        if (request.Rating < 1 || request.Rating > 10)
        {
            return BadRequest("Rating must be between 1 and 10.");
        }

        review.Rating = request.Rating;
        review.Comment = request.Comment;

        _context.SaveChanges();

        var result = new ReviewDTO
        {
            Id = review.Id,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt,
            UserId = review.UserId,
            Username = review.User != null ? review.User.Username : string.Empty,
            MovieId = review.MovieId,
            MovieTitle = review.Movie != null ? review.Movie.Title : string.Empty
        };

        return Ok(result);
    }

    [HttpDelete("delete/{id}")]
    [Authorize]
    public IActionResult DeleteReview(int id)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();
        var currentUserId = int.Parse(userIdClaim.Value);

        var review = _context.Reviews.FirstOrDefault(x => x.Id == id);
        if (review == null)
        {
            return NotFound();
        }

        if (review.UserId != currentUserId)
        {
            return Forbid("You can only delete your own reviews.");
        }

        _context.Reviews.Remove(review);
        _context.SaveChanges();

        return Ok("Review deleted successfully.");
    }

    [HttpGet("get-by-movie/{movieId}")]
    public IActionResult GetReviewsByMovie(int movieId)
    {
        try
        {
            var reviews = _context.Reviews.Where(x => x.MovieId == movieId).ToList();
            var userIds = reviews.Select(x => x.UserId).Distinct().ToList();
            var users = _context.Users.Where(x => userIds.Contains(x.Id)).ToDictionary(x => x.Id, x => x.Username);

            var result = reviews.Select(x => new ReviewDTO
            {
                Id = x.Id,
                Rating = x.Rating,
                Comment = x.Comment,
                CreatedAt = x.CreatedAt,
                UserId = x.UserId,
                Username = users.ContainsKey(x.UserId) ? users[x.UserId] : "Unknown",
                MovieId = x.MovieId,
                MovieTitle = ""
            }).ToList();

            return Ok(result);
        }
        catch
        {
            return Ok(new List<ReviewDTO>());
        }
    }

    [HttpGet("get-by-user/{userId}")]
    public IActionResult GetReviewsByUser(int userId)
    {
        var reviews = _context.Reviews
            .Include(x => x.User)
            .Include(x => x.Movie)
            .Where(x => x.UserId == userId)
            .ToList();

        var result = reviews.Select(x => new ReviewDTO
        {
            Id = x.Id,
            Rating = x.Rating,
            Comment = x.Comment,
            CreatedAt = x.CreatedAt,
            UserId = x.UserId,
            Username = x.User != null ? x.User.Username : string.Empty,
            MovieId = x.MovieId,
            MovieTitle = x.Movie != null ? x.Movie.Title : string.Empty
        }).ToList();

        return Ok(result);
    }

    [HttpGet("average-rating/{movieId}")]
    public IActionResult GetAverageRating(int movieId)
    {
        var reviews = _context.Reviews.Where(x => x.MovieId == movieId).ToList();
        if (reviews.Count == 0)
        {
            return Ok(0);
        }

        var average = reviews.Average(x => x.Rating);
        return Ok(average);
    }

    [HttpGet("sort-by-rating")]
    public IActionResult SortByRating()
    {
        var reviews = _context.Reviews
            .Include(x => x.User)
            .Include(x => x.Movie)
            .OrderByDescending(x => x.Rating)
            .ToList();

        var result = reviews.Select(x => new ReviewDTO
        {
            Id = x.Id,
            Rating = x.Rating,
            Comment = x.Comment,
            CreatedAt = x.CreatedAt,
            UserId = x.UserId,
            Username = x.User != null ? x.User.Username : string.Empty,
            MovieId = x.MovieId,
            MovieTitle = x.Movie != null ? x.Movie.Title : string.Empty
        }).ToList();

        return Ok(result);
    }

    [HttpGet("count")]
    public IActionResult CountReviews()
    {
        return Ok(_context.Reviews.Count());
    }
}
