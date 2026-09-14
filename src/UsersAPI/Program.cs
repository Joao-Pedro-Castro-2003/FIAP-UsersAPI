using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Prometheus;
using System.Text;
using UsersAPI.Data;

var b = WebApplication.CreateBuilder(args);
b.Logging.ClearProviders();
b.Logging.AddJsonConsole();
b.Services.AddControllers();
b.Services.AddEndpointsApiExplorer();
b.Services.AddSwaggerGen();
b.Services.AddProblemDetails();
b.Services.AddDbContext<UsersDbContext>(o => o.UseSqlite(b.Configuration.GetConnectionString("Db")));
b.Services.AddMassTransit(x => x.UsingRabbitMq((c, q) => {
    q.Host(b.Configuration["RabbitMq:Host"] ?? "localhost", h => {
        h.Username(b.Configuration["RabbitMq:Username"] ?? "guest");
        h.Password(b.Configuration["RabbitMq:Password"] ?? "guest");
    });
    q.ConfigureEndpoints(c);
}));
var key = b.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key ausente");
if (Encoding.UTF8.GetByteCount(key) < 32) throw new InvalidOperationException("Jwt:Key precisa de ao menos 32 bytes");
b.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
    o.TokenValidationParameters = new() {
        ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true,
        ValidIssuer = b.Configuration["Jwt:Issuer"], ValidAudience = b.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), ClockSkew = TimeSpan.Zero
    });
b.Services.AddAuthorization();
var app = b.Build();
Directory.CreateDirectory("data");
using (var scope = app.Services.CreateScope()) {
    var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
    db.Database.EnsureCreated();
    // Bootstrap only a fresh database. Public signup cannot grant Admin.
    var email = b.Configuration["Bootstrap:AdminEmail"];
    var password = b.Configuration["Bootstrap:AdminPassword"];
    if (!db.Users.Any() && !string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password)) {
        if (password.Length < 12) throw new InvalidOperationException("Senha inicial precisa de 12 caracteres");
        var admin = new User { Name = "Administrador", Email = email.Trim().ToLowerInvariant(), IsAdmin = true };
        admin.PasswordHash = new PasswordHasher<User>().HashPassword(admin, password);
        db.Users.Add(admin);
        db.SaveChanges();
    }
}
app.UseRouting();
app.UseHttpMetrics();
app.UseExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/health/live", () => Results.Ok(new { status = "alive" }));
app.MapGet("/health/ready", async (UsersDbContext db) =>
    await db.Database.CanConnectAsync() ? Results.Ok(new { status = "ready" }) : Results.StatusCode(503));
app.MapMetrics();
app.MapControllers();
app.Run();
public partial class Program { }
