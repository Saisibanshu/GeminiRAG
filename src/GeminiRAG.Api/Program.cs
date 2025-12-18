using GeminiRAG.Core.Configuration;
using GeminiRAG.Core.Interfaces;
using GeminiRAG.Infrastructure.Services;
using GeminiRAG.Infrastructure.Data;
using GeminiRAG.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuration
var apiKey = builder.Configuration["GeminiApi:ApiKey"];
var storeName = builder.Configuration["FileSearchStore:StoreName"];

if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_API_KEY_HERE")
{
    // In production, you might want to throw or log a warning
    Console.WriteLine("WARNING: Gemini API Key is missing or not configured.");
}

// Database configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=ASUS_S14_OLED\\SQLEXPRESS;Database=GeminiRagDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// HttpContextAccessor for user context
builder.Services.AddHttpContextAccessor();

// JWT Configuration
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? "your-super-secret-key-minimum-32-characters-long-for-security";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "GeminiRAG";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "GeminiRAGClient";
var jwtExpiryHours = int.Parse(builder.Configuration["Jwt:ExpiryInHours"] ?? "24");

builder.Services.Configure<JwtSettings>(options =>
{
    options.SecretKey = jwtSecretKey;
    options.Issuer = jwtIssuer;
    options.Audience = jwtAudience;
    options.ExpiryInHours = jwtExpiryHours;
});

// Authentication
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey))
    };
});

// Register Services
// Note: We are manually injecting the API key here as the services expect a string in constructor
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IStoreRepository, StoreRepository>();
builder.Services.AddScoped<IQueryHistoryService, QueryHistoryService>();
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddScoped<IFileValidationService, FileValidationService>();
builder.Services.AddSingleton<IFileSearchService>(sp => new FileSearchService(apiKey ?? ""));
builder.Services.AddSingleton<IGeminiQueryService>(sp => new GeminiQueryService(apiKey ?? ""));
builder.Services.AddSingleton<IExportService, ExportService>();

// CORS configuration for Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy => policy
            .WithOrigins("http://localhost:4200") // Angular default port
            .AllowAnyMethod()
            .AllowAnyHeader());
});

builder.Services.AddHealthChecks();

var app = builder.Build();


app.MapHealthChecks("/health");

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseCors("AllowAngular");

app.UseHttpsRedirection();

app.UseAuthentication();  // Add this BEFORE UseAuthorization
app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new 
{ 
    status = "healthy",
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName
}))
.AllowAnonymous();

app.Run();
