using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using MovieManager.AppServices;
using MovieManager.DTOs;
using MovieManager.Entities;
using MovieManager.Services;
using System.Net;
using Xunit;

namespace MovieManager.Tests;

public class AuthServiceTest
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
    public async Task GetUserFromDB_ReturnsUser()
    {
        var context = GetDbContext();
        context.Users.Add(new User { Username = "UserAdmin", PasswordHash = "PassTest", RoleId = 1, Email = "test@test.com" });
        context.Users.Add(new User { Username = "UserRegular", PasswordHash = "PassTest", RoleId = 2, Email = "test@test.com" });
        context.Users.Add(new User { Username = "User2", PasswordHash = "PassTest", RoleId = 2, Email = "test@test.com" });
        await context.SaveChangesAsync();

        var service = new AuthService(context, null);

        var user = await service.GetUserFromDBAsync("UserAdmin");

        Assert.Equal(user.Username, "UserAdmin");
        Assert.Equal(user.Email, "test@test.com");
        Assert.Equal(user.PasswordHash, "PassTest");
        Assert.Equal(user.RoleId, 1);
    }

    [Fact]
    public async Task CreateUser_ShouldBeSavedOnDB()
    {
        var context = GetDbContext();
        context.Users.Add(new User { Username = "UserAdmin", PasswordHash = "PassTest", RoleId = 1, Email = "test@test.com" });
        context.Users.Add(new User { Username = "UserRegular", PasswordHash = "PassTest", RoleId = 2, Email = "test@test.com" });
        context.Users.Add(new User { Username = "User2", PasswordHash = "PassTest", RoleId = 2, Email = "test@test.com" });
        await context.SaveChangesAsync();

        var service = new AuthService(context, null);

        User newUser = new User {
            Username = "Test User",
            PasswordHash = "PasswordHashed",
            RoleId = 1,
            Email = "testUser@test.com"
        };

        await service.CreateUserAsync(newUser);

        var user = await service.GetUserFromDBAsync("Test User");
        Assert.Equal(user.Username, newUser.Username);
        Assert.Equal(user.Email, newUser.Email);
        Assert.Equal(user.PasswordHash, newUser.PasswordHash);
        Assert.Equal(user.RoleId, newUser.RoleId);
    }

}