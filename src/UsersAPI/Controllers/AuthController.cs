using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UsersAPI.Data;

namespace UsersAPI.Controllers;

[ApiController, Route("api/auth")]
public sealed class AuthController(UsersDbContext db, IConfiguration cfg) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest r)
    {
        var u = await db.Users.SingleOrDefaultAsync(x => x.Email == r.Email.ToLower());

        if (u is null || new PasswordHasher<User>().VerifyHashedPassword(u, u.PasswordHash, r.Password) == PasswordVerificationResult.Failed)
            return Unauthorized(); 
        
        var claims = new[] 
        { 
            new Claim(ClaimTypes.NameIdentifier, u.Id.ToString()), 
            new Claim(ClaimTypes.Name, u.Name), 
            new Claim(ClaimTypes.Email, u.Email), 
            new Claim(ClaimTypes.Role, u.IsAdmin ? "Admin" : "Comum") 
        }; 
        
        var c = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["Jwt:Key"]!)), SecurityAlgorithms.HmacSha256); 
        
        var t = new JwtSecurityToken(cfg["Jwt:Issuer"], cfg["Jwt:Audience"], claims, expires: DateTime.UtcNow.AddHours(8), signingCredentials: c); 
        
        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(t) });
    }
}
public sealed record LoginRequest(string Email, string Password);
