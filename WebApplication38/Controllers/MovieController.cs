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
[Route("api/movies")]
public class MovieController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public MovieController(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet("get-all")]
    public IActionResult GetAllMovies()
    {
        try
        {
            var movies = _context.Movies.ToList();
            var reviewCounts = _context.Reviews.GroupBy(r => r.MovieId)
                .Select(g => new { MovieId = g.Key, Count = g.Count(), Avg = g.Average(r => r.Rating) })
                .ToDictionary(x => x.MovieId, x => new { x.Count, x.Avg });

            var result = movies.Select(x => {
                var stats = reviewCounts.ContainsKey(x.Id) ? reviewCounts[x.Id] : null;
                var actorCount = _context.MovieActors.Count(ma => ma.MovieId == x.Id);
                return new MovieDTO
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    ReleaseYear = x.ReleaseYear,
                    Genre = x.Genre,
                    PosterUrl = x.PosterUrl,
                    AverageRating = stats?.Avg ?? 0,
                    ReviewCount = stats?.Count ?? 0,
                    ActorCount = actorCount
                };
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("get-by-id/{id}")]
    public IActionResult GetMovieById(int id)
    {
        try
        {
            var movie = _context.Movies.FirstOrDefault(x => x.Id == id);
            if (movie == null) return NotFound();

            var reviews = _context.Reviews.Where(r => r.MovieId == id).ToList();
            var actorCount = _context.MovieActors.Count(ma => ma.MovieId == id);
            var avgRating = reviews.Count > 0 ? reviews.Average(r => r.Rating) : 0;

            var result = new MovieDTO
            {
                Id = movie.Id,
                Title = movie.Title,
                Description = movie.Description,
                ReleaseYear = movie.ReleaseYear,
                Genre = movie.Genre,
                PosterUrl = movie.PosterUrl,
                AverageRating = avgRating,
                ReviewCount = reviews.Count,
                ActorCount = actorCount
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("add")]
    [Authorize(Roles = "Admin")]
    public IActionResult AddMovie(AddMovieRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest("Title is required.");
        }

        var movie = _mapper.Map<Movie>(request);
        _context.Movies.Add(movie);
        _context.SaveChanges();

        var result = new MovieDTO
        {
            Id = movie.Id,
            Title = movie.Title,
            Description = movie.Description,
            ReleaseYear = movie.ReleaseYear,
            Genre = movie.Genre,
            PosterUrl = movie.PosterUrl,
            AverageRating = 0,
            ReviewCount = 0,
            ActorCount = 0
        };

        return Ok(result);
    }

    [HttpPut("update/{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult UpdateMovie(int id, UpdateMovieRequest request)
    {
        var movie = _context.Movies.FirstOrDefault(x => x.Id == id);
        if (movie == null)
        {
            return NotFound();
        }

        movie.Title = request.Title;
        movie.Description = request.Description;
        movie.ReleaseYear = request.ReleaseYear;
        movie.Genre = request.Genre;
        movie.PosterUrl = request.PosterUrl;

        _context.SaveChanges();

        var result = _mapper.Map<MovieDTO>(movie);
        result.AverageRating = _context.Reviews.Where(x => x.MovieId == id).Any() ? _context.Reviews.Where(x => x.MovieId == id).Average(x => x.Rating) : 0;
        result.ReviewCount = _context.Reviews.Count(x => x.MovieId == id);
        result.ActorCount = _context.MovieActors.Count(x => x.MovieId == id);

        return Ok(result);
    }

    [HttpDelete("delete/{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult DeleteMovie(int id)
    {
        var movie = _context.Movies.FirstOrDefault(x => x.Id == id);
        if (movie == null)
        {
            return NotFound();
        }

        _context.Movies.Remove(movie);
        _context.SaveChanges();

        return Ok("Movie deleted successfully.");
    }

    [HttpGet("search-by-title")]
    public IActionResult SearchByTitle(string title)
    {
        var movies = _context.Movies
            .Include(x => x.MovieActors)
            .Include(x => x.Reviews)
            .Where(x => x.Title.Contains(title))
            .ToList();

        var result = movies.Select(x => new MovieDTO
        {
            Id = x.Id,
            Title = x.Title,
            Description = x.Description,
            ReleaseYear = x.ReleaseYear,
            Genre = x.Genre,
            PosterUrl = x.PosterUrl,
            AverageRating = x.Reviews.Count == 0 ? 0 : x.Reviews.Average(r => r.Rating),
            ReviewCount = x.Reviews.Count,
            ActorCount = x.MovieActors.Count
        }).ToList();

        return Ok(result);
    }

    [HttpGet("filter-by-year/{year}")]
    public IActionResult FilterByYear(int year)
    {
        var movies = _context.Movies
            .Include(x => x.MovieActors)
            .Include(x => x.Reviews)
            .Where(x => x.ReleaseYear == year)
            .ToList();

        var result = movies.Select(x => new MovieDTO
        {
            Id = x.Id,
            Title = x.Title,
            Description = x.Description,
            ReleaseYear = x.ReleaseYear,
            Genre = x.Genre,
            PosterUrl = x.PosterUrl,
            AverageRating = x.Reviews.Count == 0 ? 0 : x.Reviews.Average(r => r.Rating),
            ReviewCount = x.Reviews.Count,
            ActorCount = x.MovieActors.Count
        }).ToList();

        return Ok(result);
    }

    [HttpGet("filter-by-rating/{rating}")]
    public IActionResult FilterByRating(double rating)
    {
        var movies = _context.Movies
            .Include(x => x.MovieActors)
            .Include(x => x.Reviews)
            .ToList()
            .Where(x => x.Reviews.Count == 0 || x.Reviews.Average(r => r.Rating) >= rating)
            .Select(x => new MovieDTO
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                ReleaseYear = x.ReleaseYear,
                Genre = x.Genre,
                PosterUrl = x.PosterUrl,
                AverageRating = x.Reviews.Count == 0 ? 0 : x.Reviews.Average(r => r.Rating),
                ReviewCount = x.Reviews.Count,
                ActorCount = x.MovieActors.Count
            })
            .ToList();

        return Ok(movies);
    }

    [HttpGet("sort-by-title")]
    public IActionResult SortByTitle()
    {
        var movies = _context.Movies
            .Include(x => x.MovieActors)
            .Include(x => x.Reviews)
            .OrderBy(x => x.Title)
            .ToList();

        var result = movies.Select(x => new MovieDTO
        {
            Id = x.Id,
            Title = x.Title,
            Description = x.Description,
            ReleaseYear = x.ReleaseYear,
            Genre = x.Genre,
            PosterUrl = x.PosterUrl,
            AverageRating = x.Reviews.Count == 0 ? 0 : x.Reviews.Average(r => r.Rating),
            ReviewCount = x.Reviews.Count,
            ActorCount = x.MovieActors.Count
        }).ToList();

        return Ok(result);
    }
    
    [HttpGet("sort-by-year")]
    public IActionResult SortByYear()
    {
        var movies = _context.Movies
            .Include(x => x.MovieActors)
            .Include(x => x.Reviews)
            .OrderBy(x => x.ReleaseYear)
            .ToList();

        var result = movies.Select(x => new MovieDTO
        {
            Id = x.Id,
            Title = x.Title,
            Description = x.Description,
            ReleaseYear = x.ReleaseYear,
            Genre = x.Genre,
            PosterUrl = x.PosterUrl,
            AverageRating = x.Reviews.Count == 0 ? 0 : x.Reviews.Average(r => r.Rating),
            ReviewCount = x.Reviews.Count,
            ActorCount = x.MovieActors.Count
        }).ToList();

        return Ok(result);
    }

    [HttpGet("sort-by-rating")]
    public IActionResult SortByRating()
    {
        var movies = _context.Movies
            .Include(x => x.MovieActors)
            .Include(x => x.Reviews)
            .ToList()
            .OrderByDescending(x => x.Reviews.Count == 0 ? 0 : x.Reviews.Average(r => r.Rating))
            .Select(x => new MovieDTO
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                ReleaseYear = x.ReleaseYear,
                Genre = x.Genre,
                PosterUrl = x.PosterUrl,
                AverageRating = x.Reviews.Count == 0 ? 0 : x.Reviews.Average(r => r.Rating),
                ReviewCount = x.Reviews.Count,
                ActorCount = x.MovieActors.Count
            })
            .ToList();

        return Ok(movies);
    }

    [HttpGet("{movieId}/actors")]
    public IActionResult GetMovieActors(int movieId)
    {
        try
        {
            var movieActors = _context.MovieActors.Where(x => x.MovieId == movieId).ToList();
            var actorIds = movieActors.Select(x => x.ActorId).ToList();
            var actors = _context.Actors.Where(x => actorIds.Contains(x.Id)).ToList();

            var result = actors.Select(x => new ActorDTO
            {
                Id = x.Id,
                Name = x.Name,
                BirthDate = x.BirthDate,
                Biography = x.Biography,
                PhotoUrl = x.PhotoUrl,
                MovieCount = 0
            }).ToList();

            return Ok(result);
        }
        catch
        {
            return Ok(new List<ActorDTO>());
        }
    }

    [HttpPost("{movieId}/actors/{actorId}")]
    [Authorize(Roles = "Admin")]
    public IActionResult AddActorToMovie(int movieId, int actorId)
    {
        var existing = _context.MovieActors.FirstOrDefault(x => x.MovieId == movieId && x.ActorId == actorId);
        if (existing != null)
        {
            return BadRequest("Actor already exists in this movie.");
        }

        var movie = _context.Movies.FirstOrDefault(x => x.Id == movieId);
        var actor = _context.Actors.FirstOrDefault(x => x.Id == actorId);
        if (movie == null || actor == null)
        {
            return NotFound();
        }

        _context.MovieActors.Add(new MovieActor
        {
            MovieId = movieId,
            ActorId = actorId
        });

        _context.SaveChanges();
        return Ok("Actor added to movie successfully.");
    }

    [HttpDelete("{movieId}/actors/{actorId}")]
    [Authorize(Roles = "Admin")]
    public IActionResult RemoveActorFromMovie(int movieId, int actorId)
    {
        var relation = _context.MovieActors.FirstOrDefault(x => x.MovieId == movieId && x.ActorId == actorId);
        if (relation == null)
        {
            return NotFound();
        }

        _context.MovieActors.Remove(relation);
        _context.SaveChanges();

        return Ok("Actor removed from movie successfully.");
    }

    [HttpGet("count")]
    public IActionResult CountMovies()
    {
        return Ok(_context.Movies.Count());
    }
}
