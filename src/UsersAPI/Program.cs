using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UsersAPI.Data;

var b = WebApplication.CreateBuilder(args);

b.Services.AddControllers(); b.Services.AddEndpointsApiExplorer(); b.Services.AddSwaggerGen();

b.Services.AddDbContext<UsersDbContext>(o => o.UseSqlite(b.Configuration.GetConnectionString("Db")));

b.Services.AddMassTransit(x => x.UsingRabbitMq((c, q) => { q.Host(b.Configuration["RabbitMq:Host"] ?? "localhost", h => { h.Username("guest"); h.Password("guest"); }); q.ConfigureEndpoints(c); }));

var key = b.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key ausente");

b.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o => o.TokenValidationParameters = new() {
        ValidateIssuer = true, 
        ValidateAudience = true, 
        ValidateLifetime = true, 
        ValidateIssuerSigningKey = true, 
        ValidIssuer = b.Configuration["Jwt:Issuer"], 
        ValidAudience = b.Configuration["Jwt:Audience"], 
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)) 
    });

b.Services.AddAuthorization();

var app = b.Build(); Directory.CreateDirectory("data"); 

using (var s = app.Services.CreateScope()) 
    s.ServiceProvider.GetRequiredService<UsersDbContext>().Database.EnsureCreated();

app.UseSwagger(); 
app.UseSwaggerUI(); 
app.UseAuthentication(); 
app.UseAuthorization(); 
app.MapControllers(); 
app.Run();
public partial class Program { }
