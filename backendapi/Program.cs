using System.Text;
using backendapi.Data;
using backendapi.Models;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

Env.Load(); // Carga las variables de entorno desde el archivo .env

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure Database Connection
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Configure JWT Authentication
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? builder.Configuration["JwtKey"];
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? builder.Configuration["JwtIssuer"];
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? builder.Configuration["JwtAudience"];

builder.Configuration["JwtKey"] = jwtKey;
builder.Configuration["JwtIssuer"] = jwtIssuer;
builder.Configuration["JwtAudience"] = jwtAudience;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey ?? "SuperSecretKeyQueDeberiaEstarEnElEnvXD12345!"))
    };
    
    // Read the token from the HttpOnly Cookie
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.ContainsKey("AuthToken"))
            {
                context.Token = context.Request.Cookies["AuthToken"];
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS for Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:50637", "http://localhost:4200", "http://127.0.0.1:4200", "http://127.0.0.1:50637") // Angular default and config ports
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Make sure to allow credentials for cookies
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed Database (No automatic migrations as per user request, but we will ensure Database gets seeded if the user manually creates db/tables).
// The user will generate migrations. We will just attempt to seed if the user created the tables.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        // Try avoiding EF Create to not generate schema if it relies on migrations but let's seed data only if EF context connects
        if (context.Database.CanConnect())
        {
            if (!context.Usuarios.Any())
            {
                var seedUsername = Environment.GetEnvironmentVariable("SEED_USERNAME") ?? "admin";
                var seedPassword = Environment.GetEnvironmentVariable("SEED_PASSWORD") ?? "admin123";
                
                context.Usuarios.Add(new Usuario
                {
                    nombre = "Seed",
                    apellido = "User",
                    fecha_nacimiento = new DateTime(1990, 1, 1),
                    correo = "admin@system.com",
                    activo = true,
                    username = seedUsername,
                    passwordhash = BCrypt.Net.BCrypt.HashPassword(seedPassword)
                });
                context.SaveChanges();
            }
        }
    }
    catch (Exception)
    {
        // Ignorar si la base de datos o tabla no existe todavía
    }
}

app.Run();
