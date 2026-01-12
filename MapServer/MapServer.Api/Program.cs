using MapServer.Application.Interfaces;
using MapServer.Application.Services;
using MapServer.Domain.Exceptions;
using MapServer.Domain.Interfaces;
using MapServer.Infrastructure.Configuration;
using MapServer.Infrastructure.Data;
using MapServer.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

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
builder.Services.AddSingleton<MongoDbContext>();

// Infrastructure - Scoped (repositories)
builder.Services.AddScoped<IPolygonRepository, PolygonRepository>();
builder.Services.AddScoped<IMapObjectRepository, MapObjectRepository>();

// Application - Scoped (services)
builder.Services.AddScoped<IPolygonService, PolygonService>();
builder.Services.AddScoped<IMapObjectService, MapObjectService>();

var app = builder.Build();

// Exception handling middleware - maps domain exceptions to HTTP responses.
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        if (context.Response.HasStarted)
        {
            throw;
        }

        context.Response.Clear();
        context.Response.ContentType = "application/problem+json";

        var (statusCode, title) = ex switch
        {
            EntityNotFoundException => (StatusCodes.Status404NotFound, "Not found"),
            ValidationException => (StatusCodes.Status400BadRequest, "Validation error"),
            DomainException => (StatusCodes.Status400BadRequest, "Bad request"),
            _ => (StatusCodes.Status500InternalServerError, "Server error")
        };

        context.Response.StatusCode = statusCode;

        var problem = new ProblemDetails
        {
            Title = title,
            Status = statusCode,
            Detail = statusCode == StatusCodes.Status500InternalServerError && !app.Environment.IsDevelopment()
                ? "An unexpected error occurred."
                : ex.Message
        };

        problem.Extensions["traceId"] = context.TraceIdentifier;

        await context.Response.WriteAsJsonAsync(problem);
    }
});

// Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.MapControllers();

app.Run();
