using MapServer.Api.Middleware;
using MapServer.Application.Interfaces;
using MapServer.Application.Services;
using MapServer.Infrastructure.Configuration;
using MapServer.Infrastructure.Data;
using MapServer.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// MongoDB Configuration
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

// Framework Services
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Dependency Injection Registration

// Infrastructure - Singleton (database connection)
builder.Services.AddSingleton<MongoDbContext>();//CHECK THIS

// Infrastructure - Scoped (repositories)
builder.Services.AddScoped<IPolygonRepository, PolygonRepository>();
builder.Services.AddScoped<IMapObjectRepository, MapObjectRepository>();

// Application - Scoped (services)
builder.Services.AddScoped<IPolygonService, PolygonService>();
builder.Services.AddScoped<IMapObjectService, MapObjectService>();

var app = builder.Build();

// Middleware Pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.MapControllers();

app.Run();
