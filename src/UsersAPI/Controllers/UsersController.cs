using System.ComponentModel.DataAnnotations;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FiapCloudGames.Contracts;
using UsersAPI.Data;

namespace UsersAPI.Controllers;

[ApiController, Route("api/users")]
public sealed class UsersController(UsersDbContext db, IPublishEndpoint bus) : ControllerBase
{
    [AllowAnonymous, HttpPost]
    public async Task<IActionResult> Create(CreateUserRequest r)
    {
        if (r.IsAdmin) return BadRequest("Cadastro publico nao permite administrador");
        var email = r.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.Email == email))
            return Conflict("E-mail ja cadastrado");

        var u = new User
        {
            Name = r.Name.Trim(),
            Email = email,
            PasswordHash = "",
            IsAdmin = false
        };

        u.PasswordHash = new PasswordHasher<User>().HashPassword(u, r.Password);

        db.Add(u);

        await db.SaveChangesAsync();

        await bus.Publish(new UserCreatedEvent(u.Id, u.Name, u.Email));

        return Created($"api/users/{u.Id}", new { u.Id, u.Name, u.Email, u.IsAdmin });
    }

    [Authorize(Roles = "Admin"), HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id) => await db.Users.FindAsync(id) is { } u ? Ok(new { u.Id, u.Name, u.Email, u.IsAdmin }) : NotFound();
}
public sealed record CreateUserRequest([Required, StringLength(100, MinimumLength = 2)] string Name, [Required, EmailAddress] string Email, [Required, StringLength(128, MinimumLength = 8)] string Password, bool IsAdmin = false);
