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
[Route("api/actors")]
public class ActorController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    
    public ActorController(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet("get-all")]
    public IActionResult GetAllActors()
    {
        try
        {
            var actors = _context.Actors.ToList();
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
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("get-by-id/{id}")]
    public IActionResult GetActorById(int id)
    {
        var actor = _context.Actors.Include(x => x.MovieActors).ThenInclude(x => x.Movie).FirstOrDefault(x => x.Id == id);
        if (actor == null)
        {
            return NotFound();
        }

        var result = new ActorDTO
        {
            Id = actor.Id,
            Name = actor.Name,
            BirthDate = actor.BirthDate,
            Biography = actor.Biography,
            PhotoUrl = actor.PhotoUrl,
            MovieCount = actor.MovieActors.Count
        };

        return Ok(result);
    }

    [HttpPost("add")]
    [Authorize(Roles = "Admin")]
    public IActionResult AddActor(AddActorRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Name is required.");
        }

        var actor = _mapper.Map<Actor>(request);
        _context.Actors.Add(actor);
        _context.SaveChanges();

        var result = new ActorDTO
        {
            Id = actor.Id,
            Name = actor.Name,
            BirthDate = actor.BirthDate,
            Biography = actor.Biography,
            PhotoUrl = actor.PhotoUrl,
            MovieCount = 0
        };

        return Ok(result);
    }

    [HttpPut("update/{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult UpdateActor(int id, UpdateActorRequest request)
    {
        var actor = _context.Actors.FirstOrDefault(x => x.Id == id);
        if (actor == null)
        {
            return NotFound();
        }

        actor.Name = request.Name;
        actor.BirthDate = request.BirthDate;
        actor.Biography = request.Biography;
        actor.PhotoUrl = request.PhotoUrl;

        _context.SaveChanges();

        var result = _mapper.Map<ActorDTO>(actor);
        result.MovieCount = _context.MovieActors.Count(x => x.ActorId == id);

        return Ok(result);
    }

    [HttpDelete("delete/{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult DeleteActor(int id)
    {
        var actor = _context.Actors.FirstOrDefault(x => x.Id == id);
        if (actor == null)
        {
            return NotFound();
        }

        _context.Actors.Remove(actor);
        _context.SaveChanges();

        return Ok("Actor deleted successfully.");
    }

    [HttpGet("search-by-name")]
    public IActionResult SearchByName(string name)
    {
        var actors = _context.Actors
            .Include(x => x.MovieActors)
            .Where(x => x.Name.Contains(name))
            .ToList();

        var result = actors.Select(x => new ActorDTO
        {
            Id = x.Id,
            Name = x.Name,
            BirthDate = x.BirthDate,
            Biography = x.Biography,
            PhotoUrl = x.PhotoUrl,
            MovieCount = x.MovieActors.Count
        }).ToList();

        return Ok(result);
    }

    [HttpGet("sort-by-name")]
    public IActionResult SortByName()
    {
        var actors = _context.Actors
            .Include(x => x.MovieActors)
            .OrderBy(x => x.Name)
            .ToList();

        var result = actors.Select(x => new ActorDTO
        {
            Id = x.Id,
            Name = x.Name,
            BirthDate = x.BirthDate,
            Biography = x.Biography,
            PhotoUrl = x.PhotoUrl,
            MovieCount = x.MovieActors.Count
        }).ToList();

        return Ok(result);
    }

    [HttpGet("sort-by-birthdate")]
    public IActionResult SortByBirthDate()
    {
        var actors = _context.Actors
            .Include(x => x.MovieActors)
            .OrderBy(x => x.BirthDate)
            .ToList();

        var result = actors.Select(x => new ActorDTO
        {
            Id = x.Id,
            Name = x.Name,
            BirthDate = x.BirthDate,
            Biography = x.Biography,
            PhotoUrl = x.PhotoUrl,
            MovieCount = x.MovieActors.Count
        }).ToList();

        return Ok(result);
    }

    [HttpGet("{actorId}/movies")]
    public IActionResult GetActorMovies(int actorId)
    {
        var movies = _context.MovieActors
            .Where(x => x.ActorId == actorId)
            .Include(x => x.Movie)
            .Select(x => x.Movie)
            .Where(x => x != null)
            .Select(x => new MovieDTO
            {
                Id = x!.Id,
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

    [HttpGet("count")]
    public IActionResult CountActors()
    {
        return Ok(_context.Actors.Count());
    }
}
