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

        /// <summary>
        /// Get all movies.
        /// </summary>
        /// <response code="200">Returns the list of movies</response>
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

        /// <summary>
        /// Get a movie by ID (Regular only).
        /// </summary>
        /// <param name="id">Movie ID</param>
        /// <response code="200">Returns the movie</response>
        /// <response code="404">Movie not found</response>
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

        /// <summary>
        /// Add a new movie (Admin only).
        /// </summary>
        /// <remarks>
        /// Example request:
        /// {
        ///   "title": "The Batman",
        ///   "director": "Matt Reeves",
        ///   "releaseYear": "2022",
        ///   "description": "A darker take on the Batman story."
        /// }
        /// </remarks>
        /// <response code="201">Movie created successfully</response>
        /// <response code="400">Invalid request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden (only Admins can add movies)</response>
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

        /// <summary>
        /// Update an existing movie (Admin only).
        /// </summary>
        /// <param name="id">Movie ID</param>
        /// <remarks>
        /// Example request:
        /// {
        ///   "title": "The Batman",
        ///   "director": "Matt Reeves",
        ///   "releaseYear": "2022",
        ///   "description": "Updated description."
        /// }
        /// </remarks>
        /// <response code="200">Movie updated successfully</response>
        /// <response code="400">Invalid request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden (only Admins can update movies)</response>
        /// <response code="404">Movie not found</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<MovieResponseDTO>> UpdateMovie(int id, [FromBody] UpdateMovieDTO movie)
        {
            try
            {
                var updateMovie = new Movie
                {
                    Title = movie.Title,
                    ReleaseYear = movie.ReleaseYear,
                    Director = movie.Director,
                    Description = movie.Description,
                };

                var existingMovie = await _movieService.UpdateMovieAsync(id, updateMovie);
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

        /// <summary>
        /// Delete a movie (Admin only).
        /// </summary>
        /// <param name="id">Movie ID</param>
        /// <response code="200">Movie deleted successfully</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden (only Admins can delete movies)</response>
        /// <response code="404">Movie not found</response>
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

        /// <summary>
        /// Synchronize Star Wars movies into the database.
        /// </summary>
        /// <response code="200">If the movies were added successfully</response>
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
