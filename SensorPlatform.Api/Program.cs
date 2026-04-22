using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SensorPlatform.Application.Data;
using SensorPlatform.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddScoped<SensorService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowBlazor");

// viktigt att ha UseAuthentication innan UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();