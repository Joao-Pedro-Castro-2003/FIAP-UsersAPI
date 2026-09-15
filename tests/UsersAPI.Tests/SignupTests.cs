using UsersAPI.Controllers;
using UsersAPI.Data;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
namespace UsersAPI.Tests;
public class SignupTests
{
    [Fact]
    public async Task PublicSignupRejectsAdminAndHashesNormalUsersPassword()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        using var db = new UsersDbContext(new DbContextOptionsBuilder<UsersDbContext>().UseSqlite(connection).Options);
        db.Database.EnsureCreated();
        var controller = new UsersController(db, new Mock<IPublishEndpoint>().Object);
        Assert.IsType<BadRequestObjectResult>(await controller.Create(new("Admin", "admin@example.com", "Password123", true)));
        Assert.Empty(db.Users);
        Assert.IsType<CreatedResult>(await controller.Create(new(" Player ", "PLAYER@example.com", "Password123")));
        var user = await db.Users.SingleAsync();
        Assert.False(user.IsAdmin);
        Assert.Equal("player@example.com", user.Email);
        Assert.Equal("Player", user.Name);
        Assert.NotEqual("Password123", user.PasswordHash);
        Assert.NotEqual(PasswordVerificationResult.Failed,
            new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, "Password123"));
        Assert.IsType<ConflictObjectResult>(await controller.Create(new("Player", " PLAYER@example.com ", "Password123")));
    }
}
