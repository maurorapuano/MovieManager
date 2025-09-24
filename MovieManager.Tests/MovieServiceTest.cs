using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using MovieManager.DTOs;
using MovieManager.Entities;
using MovieManager.Services;
using System.Net;
using Xunit;

namespace MovieManager.Tests;

public class MovieServiceTest
{
    private AppDbContext GetDbContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);

        context.Database.EnsureCreated();

        return context;
    }

    [Fact]
    public async Task GetMovies_ReturnsAllMovies()
    {
        var context = GetDbContext();
        context.Movies.Add(new Movie { Title = "Movie1", Director = "Director1", ReleaseYear = "2023" });
        context.Movies.Add(new Movie { Title = "Movie2", Director = "Director2", ReleaseYear = "2022" });
        context.Movies.Add(new Movie { Title = "Movie3", Director = "Director3", ReleaseYear = "2021" });
        await context.SaveChangesAsync();

        var service = new MovieService(context, null, null);

        var movies = await service.GetMovies();

        Assert.Equal(3, movies.Count);
        Assert.Contains(movies, m => m.Title == "Movie1");
        Assert.Contains(movies, m => m.Title == "Movie2");
        Assert.Contains(movies, m => m.Title == "Movie3");
    }

    [Fact]
    public async Task GetMovieById_ReturnsCorrectMovie()
    {
        var context = GetDbContext();
        var movie1 = new Movie { Title = "Movie1", Director = "Director1", ReleaseYear = "2023" };
        var movie2 = new Movie { Title = "Movie2", Director = "Director2", ReleaseYear = "2022" };
        var movie3 = new Movie { Title = "Movie3", Director = "Director3", ReleaseYear = "2021" };
        context.Movies.Add(movie1);
        context.Movies.Add(movie2);
        context.Movies.Add(movie3);
        await context.SaveChangesAsync();

        var service = new MovieService(context, null, null);

        var movie = await service.GetMovieById(movie2.Id);

        Assert.Equal(movie2.Id, movie.Id);
        Assert.Equal(movie2.Title, movie.Title);
        Assert.Equal(movie2.Director, movie.Director);
        Assert.Equal(movie2.ReleaseYear, movie.ReleaseYear);
        Assert.Equal(movie2.Description, movie.Description);
    }

    [Fact]
    public async Task UpdateMovie_ShouldChangeData()
    {
        var context = GetDbContext();
        var movie1 = new Movie { Title = "Movie1", Director = "Director1", ReleaseYear = "2023" };
        var movie2 = new Movie { Title = "Movie2", Director = "Director2", ReleaseYear = "2022" };
        var movie3 = new Movie { Title = "Movie3", Director = "Director3", ReleaseYear = "2021" };
        context.Movies.Add(movie1);
        context.Movies.Add(movie2);
        context.Movies.Add(movie3);
        await context.SaveChangesAsync();

        var updateMovie = new Movie
        {
            Title = "The Batman",
            Director = "Matt Reeves",
            ReleaseYear = "2022",
            Description = "Test Desc"
        };

        var service = new MovieService(context, null, null);

        var updatedMovie = await service.UpdateMovieAsync(movie2.Id, updateMovie);

        //Validate updated movie
        Assert.NotNull(updatedMovie);
        Assert.Equal(updateMovie.Title, updatedMovie.Title);
        Assert.Equal(updateMovie.Director, updatedMovie.Director);
        Assert.Equal(updateMovie.ReleaseYear, updatedMovie.ReleaseYear);
        Assert.Equal(updateMovie.Description, updatedMovie.Description);

        //Validate saving to DB
        var movie = context.Movies.FirstOrDefault(m => m.Title == updatedMovie.Title);
        Assert.NotNull(movie);
        Assert.Equal(updateMovie.Title, movie.Title);
        Assert.Equal(updateMovie.Director, movie.Director);
        Assert.Equal(updateMovie.ReleaseYear, movie.ReleaseYear);
        Assert.Equal(updateMovie.Description, movie.Description);
    }

    [Fact]
    public async Task CreateMovie_ShouldSaveToDB()
    {
        var dbContext = GetDbContext();
        var httpClient = new HttpClient();
        var configuration = new ConfigurationBuilder().Build();

        var service = new MovieService(dbContext, httpClient, configuration);

        var newMovie = new Movie
        {
            Title = "The Batman",
            Director = "Matt Reeves",
            ReleaseYear = "2022"
        };
        //Save to DB
        var createdMovie = await service.CreateMovieAsync(newMovie);

        //Validate created movie
        Assert.NotNull(createdMovie);
        Assert.Equal("The Batman", createdMovie.Title);
        Assert.Equal("Matt Reeves", createdMovie.Director);
        Assert.Equal("2022", createdMovie.ReleaseYear);

        //Validate saving to DB
        var movie = dbContext.Movies.FirstOrDefault(m => m.Title == "The Batman");
        Assert.NotNull(movie);
        Assert.Equal("Matt Reeves", movie.Director);
        Assert.Equal("2022", movie.ReleaseYear);
    }

    [Fact]
    public async Task DeleteMovie_ShouldRemoveFromDB()
    {
        var context = GetDbContext();
        var movie1 = new Movie { Title = "Movie1", Director = "Director1", ReleaseYear = "2023" };
        var movie2 = new Movie { Title = "Movie2", Director = "Director2", ReleaseYear = "2022" };
        var movie3 = new Movie { Title = "Movie3", Director = "Director3", ReleaseYear = "2021" };
        context.Movies.Add(movie1);
        context.Movies.Add(movie2);
        context.Movies.Add(movie3);
        await context.SaveChangesAsync();

        var service = new MovieService(context, null, null);

        var deletedMovie = await service.DeleteMovieAsync(movie2.Id);
        Assert.Equal(deletedMovie, movie2.Title);

        var movies = await service.GetMovies();
        Assert.Equal(2, movies.Count);
        Assert.Contains(movies, m => m.Title == "Movie1");
        Assert.Contains(movies, m => m.Title == "Movie3");
    }

    [Fact]
    public async Task GetStarWarsMovies_ShouldReturnAllMovies()
    {
        var mockJson = @"
        {
          ""message"": ""ok"",
          ""result"": [
            { ""_id"": ""1"", ""uid"": ""1"", ""properties"": { ""title"": ""Movie1"", ""director"": ""Director1"", ""release_date"": ""2023-01-01"", ""opening_crawl"": ""Desc1"" } },
            { ""_id"": ""2"", ""uid"": ""2"", ""properties"": { ""title"": ""Movie2"", ""director"": ""Director2"", ""release_date"": ""2022-01-01"", ""opening_crawl"": ""Desc2"" } },
            { ""_id"": ""3"", ""uid"": ""3"", ""properties"": { ""title"": ""Movie3"", ""director"": ""Director3"", ""release_date"": ""2020-01-01"", ""opening_crawl"": ""Desc3"" } },
            { ""_id"": ""4"", ""uid"": ""4"", ""properties"": { ""title"": ""Movie4"", ""director"": ""Director4"", ""release_date"": ""2021-01-01"", ""opening_crawl"": ""Desc4"" } },
            { ""_id"": ""5"", ""uid"": ""5"", ""properties"": { ""title"": ""Movie5"", ""director"": ""Director5"", ""release_date"": ""2019-01-01"", ""opening_crawl"": ""Desc5"" } }
          ]
        }";

        var context = GetDbContext();
        var httpClient = GetMockHttpClient(mockJson);
        var configuration = new Mock<IConfiguration>();
        configuration.Setup(c => c["ExternalAPI:StarWars"]).Returns("https://mockapi.com/");
        var service = new MovieService(context, httpClient, configuration.Object);

        var movies = await service.GetStarWarsMovies();

        var moviesSaved = await service.GetMovies();

        Assert.Equal(5, movies.Count);
        Assert.Contains(movies, m => m.Title == "Movie1" && m.Director == "Director1" && m.ReleaseYear == "2023" && m.Description == "Desc1");
        Assert.Contains(movies, m => m.Title == "Movie2" && m.Director == "Director2" && m.ReleaseYear == "2022" && m.Description == "Desc2");
        Assert.Contains(movies, m => m.Title == "Movie3" && m.Director == "Director3" && m.ReleaseYear == "2020" && m.Description == "Desc3");
        Assert.Contains(movies, m => m.Title == "Movie4" && m.Director == "Director4" && m.ReleaseYear == "2021" && m.Description == "Desc4");
        Assert.Contains(movies, m => m.Title == "Movie5" && m.Director == "Director5" && m.ReleaseYear == "2019" && m.Description == "Desc5");
    }

    [Fact]
    public async Task SaveStarWarsMovies_OnlySaveIfNotExists()
    {
        var context = GetDbContext();
        var existingMovie = new Movie { Title = "Movie1", Director = "Director1", ReleaseYear = "2023" };
        context.Movies.Add(existingMovie);
        await context.SaveChangesAsync();

        var moviesFromApi = new List<Movie>
        {
            new Movie { Title = "Movie1", Director = "Director1", ReleaseYear = "2023" }, 
            new Movie { Title = "Movie2", Director = "Director2", ReleaseYear = "2022" }, 
            new Movie { Title = "Movie3", Director = "Director3", ReleaseYear = "2021" }  
        };

        var service = new MovieService(context, null, null);

        foreach (var movie in moviesFromApi)
        {
            if (!context.Movies.Any(m => m.Title == movie.Title))
            {
                context.Movies.Add(movie);
            }
        }
        await context.SaveChangesAsync();

        var allMovies = context.Movies.ToList();
        Assert.Equal(3, allMovies.Count); 
        Assert.Contains(allMovies, m => m.Title == "Movie1");
        Assert.Contains(allMovies, m => m.Title == "Movie2");
        Assert.Contains(allMovies, m => m.Title == "Movie3");
    }

    private HttpClient GetMockHttpClient(string jsonResponse)
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse),
            });

        return new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("http://test.com/") 
        };
    }
}