using GeminiRAG.Core.Configuration;
using GeminiRAG.Core.Interfaces;
using GeminiRAG.Infrastructure.Services;

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

// Register Services
// Note: We are manually injecting the API key here as the services expect a string in constructor
builder.Services.AddSingleton<IFileSearchService>(sp => new FileSearchService(apiKey ?? ""));
builder.Services.AddSingleton<IGeminiQueryService>(sp => new GeminiQueryService(apiKey ?? ""));
builder.Services.AddSingleton<IQueryHistoryService, QueryHistoryService>();
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
