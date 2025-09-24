using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MovieManager.DTOs;
using MovieManager.Entities;
using MovieManager.Services;
using MovieManager.Validators;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MovieManager.Controllers
{
    [Route("api/movies")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly MovieService _movieService;

        public MoviesController(AppDbContext context, IConfiguration configuration, MovieService movieService)
        {
            _context = context;
            _configuration = configuration;
            _movieService = movieService;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<MovieResponseDTO>>> GetMovies()
        {
            try
            {
                var movies = await _movieService.GetMovies();
                return Ok(movies);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Regular")]
        public async Task<ActionResult<MovieResponseDTO>> GetMovieById(int id)
        {
            try
            {
                var existingMovie = await _movieService.GetMovieById(id);
                if (existingMovie == null)
                {
                    return NotFound(new { message = "Movie not found." });
                }

                return Ok(existingMovie);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<MovieResponseDTO>> CreateMovie([FromBody] MovieDTO movie)
        {
            try
            {
                var error = MovieValidator.Validate(movie.Title, movie.Director, movie.ReleaseYear, movie.Description);
                if (error != null)
                    return BadRequest(error);

                var newMovie = new Movie
                {
                    Title = movie.Title,
                    ReleaseYear = movie.ReleaseYear,
                    Director = movie.Director,
                    Description = movie.Description,
                };

                var existingMovie = await _movieService.GetMovieByData(newMovie.Title, newMovie.Director, newMovie.ReleaseYear);
                if (existingMovie != null)
                {
                    return Ok(new { message = "Movie already exists." });
                }

                var newMovieCreated = await _movieService.CreateMovieAsync(newMovie);

                return CreatedAtAction(nameof(GetMovieById), new { id = newMovieCreated.Id }, newMovieCreated);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<MovieResponseDTO>> UpdateMovie(int id, [FromBody] MovieDTO movie)
        {
            try
            {
                var searchMovie = await _movieService.CreateMovieAsync(new Movie
                {
                    Title = movie.Title,
                    ReleaseYear = movie.ReleaseYear,
                    Director = movie.Director,
                    Description = movie.Description,
                });

                var existingMovie = await _movieService.UpdateMovieAsync(id, searchMovie);
                if(existingMovie == null)
                {
                    return NotFound(new { message = "Movie not found." });
                }

                return CreatedAtAction(nameof(GetMovieById), new { id = existingMovie.Id }, existingMovie);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteMovie(int id)
        {
            try
            {
                string existingMovie = await _movieService.DeleteMovieAsync(id);
                if (existingMovie == null)
                {
                    return NotFound(new { message = "Movie not found." });
                }

                return Ok(new { message = "Movie removed succesfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("sync")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> SyncStarWarsMovies()
        {
            try
            {
                List<Movie> movies = new List<Movie>();
                movies = await _movieService.GetStarWarsMovies();
                if(movies == null)
                {
                    return BadRequest(new { message = "Error while getting Star Wars movies." });
                }

                string res = await _movieService.SaveStarWarsMoviesAsync(movies);
                
                return Ok(new { message = "Movies synced succesfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
