using System.Runtime.ExceptionServices;
using MapServer.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace MapServer.Api.Middleware;

/// <summary>
/// Global exception handling middleware.
/// Maps exceptions to RFC 7807 ProblemDetails responses with user-friendly messages.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(RequestDelegate next, IHostEnvironment environment)
    {
        _next = next;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        if (context.Response.HasStarted)
            ExceptionDispatchInfo.Throw(ex);  // Preserve stack trace

        var (statusCode, title, detail) = MapException(ex);

        context.Response.Clear();
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        var problem = new ProblemDetails
        {
            Title = title,
            Status = statusCode,
            Detail = detail
        };
        problem.Extensions["traceId"] = context.TraceIdentifier;

        await context.Response.WriteAsJsonAsync(problem);
    }

    private (int statusCode, string title, string detail) MapException(Exception ex)
    {
        return ex switch
        {
            // Domain exceptions - use exception message directly
            EntityNotFoundException e => (
                StatusCodes.Status404NotFound,
                "Not found",
                e.Message),

            ValidationException e => (
                StatusCodes.Status400BadRequest,
                "Validation error",
                e.Message),

            DomainException e => (
                StatusCodes.Status400BadRequest,
                "Bad request",
                e.Message),

            // MongoDB geometry validation errors - translate to user-friendly message
            MongoWriteException e when IsGeometryError(e) => (
                StatusCodes.Status400BadRequest,
                "Invalid geometry",
                "The provided geometry is invalid. Polygon edges must not cross each other (self-intersection)."),

            MongoBulkWriteException e when IsGeometryError(e) => (
                StatusCodes.Status400BadRequest,
                "Invalid geometry",
                "One or more geometries are invalid. Polygon edges must not cross each other (self-intersection)."),

            // Other MongoDB errors - generic database error
            MongoException => (
                StatusCodes.Status500InternalServerError,
                "Database error",
                _environment.IsDevelopment() ? ex.Message : "A database error occurred."),

            // Unknown errors
            _ => (
                StatusCodes.Status500InternalServerError,
                "Server error",
                _environment.IsDevelopment() ? ex.Message : "An unexpected error occurred.")
        };
    }

    private static bool IsGeometryError(MongoWriteException ex)
    {
        if (ex.WriteError?.Code == 16755)
            return true;
        var message = ex.WriteError?.Message ?? string.Empty;
        return message.Contains("Loop is not valid", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Can't extract geo keys", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsGeometryError(MongoBulkWriteException ex)
    {
        return ex.WriteErrors.Any(e =>
            e.Code == 16755 ||
            (e.Message?.Contains("Can't extract geo keys", StringComparison.OrdinalIgnoreCase) ?? false));
    }
}
