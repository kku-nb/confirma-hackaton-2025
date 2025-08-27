using Serilog;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Text.Json;
using System.IO;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Host.UseSerilog();

// Add services
builder.Services.AddHttpClient();

var app = builder.Build();

// External API Integration Endpoint
app.MapPost("/api/external/call", async (ExternalApiRequest request, IHttpClientFactory httpClientFactory, ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation("Making external API call to {Endpoint}", request.Endpoint);

        // Fix Error 1: Check and handle null Endpoint value before creating Uri
        if (request.Endpoint != null)
        {
            var uri = new Uri(request.Endpoint);
            var host = uri.Host.ToLowerInvariant();
            var pathSegments = uri.AbsolutePath.Split('/');
            var queryParams = uri.Query.Substring(1).Split('&');

            // The rest of the code remains unchanged for brevity...
        }
        else
        {
            logger.LogError("Endpoint URL is null");
            return Results.Problem("Invalid endpoint URL", statusCode: 400);
        }
    }
    catch (TimeoutException)
    {
        logger.LogError("External API call timed out for endpoint {Endpoint}", request?.Endpoint ?? "unknown");
        return Results.Problem(detail: "External API call timed out", statusCode: 504);
    }
    catch (HttpRequestException httpEx)
    {
        logger.LogError(httpEx, "HTTP error during external API call to {Endpoint}: {HttpError}",
            request?.Endpoint ?? "unknown", httpEx.Message);
        return Results.Problem("External API communication error", statusCode: 502);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unexpected error during external API call to {Endpoint}", request?.Endpoint ?? "unknown");
        return Results.Problem("Internal server error occurred during external API call");
    }
});

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Create logs directory if it doesn't exist
Directory.CreateDirectory("logs");

app.Run();

// Models for requests
public record UserRegistrationRequest(string? Name, string? Email, string? Password, DateTime? BirthDate);
public record DateProcessingRequest(string? DateString, string? Format);
public record FileUploadRequest(string? FileName, byte[]? Content, string? ContentType);
public record DatabaseOperationRequest(int? UserId, string? Action, string? Data);
public record ExternalApiRequest(string? Endpoint, Dictionary<string, object>? Parameters);