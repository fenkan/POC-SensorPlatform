using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SensorPlatform.Application.Data;
using SensorPlatform.Application.Services;
using SensorPlatform.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddScoped<SensorService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IngestService>();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<AppDataSeeder>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=app.db"));

// Konfigurerar JWT-autentisering för API:t
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Hämtar och validerar JWT-nyckeln från appsettings (måste finnas)
        var jwtKey = builder.Configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is missing");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Kontroll av vem som utfärdat token
            ValidateIssuer = true,

            // Kontroll av mottagare (vem token är för)
            ValidateAudience = true,

            // Kontroll att token inte är utgången
            ValidateLifetime = true,

            // Kontroll av signatur (säkerhet)
            ValidateIssuerSigningKey = true,

            // Vem som får skapa token
            ValidIssuer = builder.Configuration["Jwt:Issuer"],

            // Vem token är giltig för
            ValidAudience = builder.Configuration["Jwt:Audience"],

            // Krypteringsnyckel för att validera JWT-signaturen
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// CORS (för Blazor)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        var origins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ??
            ["http://localhost:5190", "https://localhost:7216"];

        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    var seeder = scope.ServiceProvider.GetRequiredService<AppDataSeeder>();
    await seeder.SeedAsync();
}

// Pipeline
if (app.Environment.IsDevelopment())
{
}

app.UseCors("AllowBlazor");

// viktigt att ha UseAuthentication innan UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();