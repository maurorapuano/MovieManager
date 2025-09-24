using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MovieManager.DTOs;
using MovieManager.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace MovieManager.Services
{
    public class MovieService
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        public MovieService(AppDbContext appDbContext, HttpClient httpClient, IConfiguration configuration) 
        {
            _context = appDbContext;
            _httpClient = httpClient;
            _configuration = configuration;
        }
        public async Task<List<Movie>> GetMovies()
        {
            List<Movie> movies = await _context.Movies.ToListAsync();

            return movies;
        }

        public async Task<Movie> GetMovieById(int id)
        {
            Movie existingMovie = await _context.Movies.FindAsync(id);

            return existingMovie;
        }

        public async Task<Movie> UpdateMovieAsync(int id, Movie movie)
        {
            Movie existingMovie = await _context.Movies.FindAsync(id);
            if (existingMovie == null)
            {
                return null;
            }

            existingMovie.Title = movie.Title;
            existingMovie.Director = movie.Director;
            existingMovie.ReleaseYear = movie.ReleaseYear;
            existingMovie.Description = movie.Description;

            _context.Movies.Update(existingMovie);
            await _context.SaveChangesAsync();

            return existingMovie;
        }

        public async Task<Movie> CreateMovieAsync(Movie movie)
        {
            Movie newMovie = new Movie
            {
                Title = movie.Title,
                Director = movie.Director,
                ReleaseYear = movie.ReleaseYear,
                Description = movie.Description,
            };

            _context.Movies.Add(newMovie);
            await _context.SaveChangesAsync();

            return newMovie;
        }

        public async Task<string> DeleteMovieAsync(int id)
        {
            Movie existingMovie = await _context.Movies.FindAsync(id);
            if (existingMovie == null)
            {
                return null;
            }

            _context.Movies.Remove(existingMovie);
            await _context.SaveChangesAsync();

            return existingMovie.Title;
        }

        public async Task<List<Movie>> GetStarWarsMovies()
        {
            try
            {
                string apiUrl = _configuration["ExternalAPI:StarWars"].ToString() + "films";
                var response = await _httpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var starWarsFilms = JsonSerializer.Deserialize<StarWarsFilmResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true});

                return starWarsFilms.Result.Select(f => new Movie
                {
                    Title = f.Properties.Title,
                    Director = f.Properties.Director,
                    Description = f.Properties.Description,
                    ReleaseYear = f.Properties.ReleaseDate.Substring(0, 4)
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while getting Star Wars movies", ex);
            }
        }

        public async Task<string> SaveStarWarsMoviesAsync(List<Movie> movies)
        {
            try
            {
                var existingTitles = _context.Movies
                    .Select(m => m.Title)
                    .ToHashSet();

                var newMovies = movies
                    .Where(dto => !existingTitles.Contains(dto.Title))
                    .Select(dto => new Movie
                    {
                        Title = dto.Title,
                        Director = dto.Director,
                        Description = dto.Description,
                        ReleaseYear = dto.ReleaseYear
                    }).ToList();

                _context.Movies.AddRange(newMovies);
                await _context.SaveChangesAsync();

                return "Movies saved succesfully.";
            }
            catch (Exception ex)
            {
                throw new Exception("Error while saving new Star Wars movies.", ex);
            }
        }
    }
}
