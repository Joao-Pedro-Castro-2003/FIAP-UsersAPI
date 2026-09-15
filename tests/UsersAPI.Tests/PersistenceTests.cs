using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using UsersAPI.Data;

namespace UsersAPI.Tests;

public class PersistenceTests
{
    [Fact]
    public async Task CreatesSchemaAndPersistsUserAcrossContexts()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<UsersDbContext>().UseSqlite(connection).Options;
        await using (var db = new UsersDbContext(options))
        {
            await db.Database.EnsureCreatedAsync();
            db.Users.Add(new User { Name = "Test", Email = "test@example.com", PasswordHash = "hashed" });
            await db.SaveChangesAsync();
        }
        await using var reopened = new UsersDbContext(options);
        var user = await reopened.Users.SingleAsync();
        Assert.True(user.Id > 0);
        Assert.Equal("hashed", user.PasswordHash);
        Assert.False(user.IsAdmin);
        reopened.Users.Add(new User { Name = "Duplicate", Email = user.Email, PasswordHash = "other" });
        await Assert.ThrowsAsync<DbUpdateException>(() => reopened.SaveChangesAsync());
    }
}
