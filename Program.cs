using Serilog;
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

// 1. User Registration Endpoint - NullReferenceException on missing data
app.MapPost("/api/users/register", async (UserRegistrationRequest request, ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation("Starting user registration for {Email}", request.Email);

        // Error 1: NullReferenceException when trying to process name operations
        logger.LogInformation("Processing user name normalization...");
        var normalizedName = request.Name.Trim().ToUpperInvariant(); // Will throw NullReferenceException if Name is null
        var nameInitials = $"{normalizedName[0]}.{normalizedName.Split(' ')[1][0]}."; // IndexOutOfRangeException if no space
        
        // Error 2: Email domain extraction without validation 
        logger.LogInformation("Extracting email domain...");
        var emailParts = request.Email.Split('@'); // NullReferenceException if Email is null
        var domain = emailParts[1].ToLowerInvariant(); // IndexOutOfRangeException if no @ symbol
        var domainValidation = domain.Contains(".") ? "valid" : throw new FormatException("Invalid domain format");
        
        // Error 3: Password strength analysis
        logger.LogInformation("Analyzing password strength...");
        var hasUpperCase = request.Password.Any(char.IsUpper); // NullReferenceException if Password is null
        var hasLowerCase = request.Password.Any(char.IsLower);
        var hasDigits = request.Password.Any(char.IsDigit);
        var strengthScore = (hasUpperCase ? 1 : 0) + (hasLowerCase ? 1 : 0) + (hasDigits ? 1 : 0);
        
        // Error 4: Birth date calculations
        logger.LogInformation("Calculating user age and zodiac sign...");
        var age = DateTime.Now.Year - request.BirthDate.Value.Year; // NullReferenceException if BirthDate is null
        var dayOfYear = request.BirthDate.Value.DayOfYear;
        var zodiacSign = dayOfYear < 80 ? "Aquarius" : "Other"; // Simplified zodiac calculation
        
        // Error 5: User preferences initialization based on data
        logger.LogInformation("Setting up user preferences...");
        var preferences = new Dictionary<string, object>
        {
            {"display_name", normalizedName},
            {"email_domain", domain},
            {"password_strength", strengthScore},
            {"age_group", age < 18 ? "minor" : age < 65 ? "adult" : "senior"},
            {"zodiac", zodiacSign}
        };

        logger.LogInformation("User registration successful for {Email} with name {Name}", request.Email, request.Name);
        return Results.Ok(new { 
            message = "User registered successfully", 
            userId = Random.Shared.Next(1000, 9999),
            preferences = preferences
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unexpected error during user registration for {Email}", request?.Email ?? "unknown");
        return Results.Problem("Internal server error occurred during registration");
    }
});

// 2. Date Processing Endpoint - FormatException and ArgumentException
app.MapPost("/api/dates/process", async (DateProcessingRequest request, ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation("Processing date string: {DateString} with format: {Format}", request.DateString, request.Format);

        // Error 1: Direct string manipulation without null checks
        logger.LogInformation("Normalizing date string...");
        var cleanDateString = request.DateString.Trim().Replace("  ", " "); // NullReferenceException if DateString is null
        
        // Error 2: Format string operations
        logger.LogInformation("Processing format string...");
        var formatParts = request.Format.Split('-', '/', ' '); // NullReferenceException if Format is null
        var formatLength = formatParts.Length;
        var primaryFormat = formatParts[0]; // IndexOutOfRangeException if empty array
        
        // Error 3: Direct DateTime parsing without validation
        logger.LogInformation("Parsing date with exact format...");
        var parsedDate = DateTime.ParseExact(cleanDateString, request.Format, CultureInfo.InvariantCulture); // FormatException if invalid
        
        // Error 4: Date arithmetic operations
        logger.LogInformation("Calculating date properties...");
        var daysSinceEpoch = (parsedDate - new DateTime(1970, 1, 1)).Days;
        var weekNumber = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(parsedDate, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        var quarterInfo = $"Q{(parsedDate.Month - 1) / 3 + 1}-{parsedDate.Year}";
        
        // Error 5: String formatting operations
        logger.LogInformation("Generating formatted outputs...");
        var formattedOutputs = new Dictionary<string, string>
        {
            {"iso8601", parsedDate.ToString("yyyy-MM-ddTHH:mm:ssZ")},
            {"friendly", parsedDate.ToString("MMMM dd, yyyy")},
            {"short", parsedDate.ToString("MM/dd/yy")},
            {"custom", parsedDate.ToString(request.Format)} // FormatException if invalid format
        };

        logger.LogInformation("Date processing successful: {DateString} parsed to {ParsedDate}", request.DateString, parsedDate);
        return Results.Ok(new { 
            originalDate = request.DateString,
            originalFormat = request.Format,
            parsedDate = parsedDate,
            dayOfWeek = parsedDate.DayOfWeek.ToString(),
            isWeekend = parsedDate.DayOfWeek == DayOfWeek.Saturday || parsedDate.DayOfWeek == DayOfWeek.Sunday,
            daysSinceEpoch = daysSinceEpoch,
            weekNumber = weekNumber,
            quarter = quarterInfo,
            formattedOutputs = formattedOutputs
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unexpected error during date processing for {DateString}", request?.DateString ?? "unknown");
        return Results.Problem("Internal server error occurred during date processing");
    }
});

// 3. File Upload Endpoint - Array/Collection Operation Errors
app.MapPost("/api/files/upload", async (FileUploadRequest request, ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation("Processing file upload: {FileName}", request.FileName);

        // Error 1: String operations on filename without null check
        logger.LogInformation("Analyzing filename structure...");
        var fileNameParts = request.FileName.Split('.'); // NullReferenceException if FileName is null
        var baseName = fileNameParts[0]; // IndexOutOfRangeException if no dots
        var extension = fileNameParts[fileNameParts.Length - 1].ToLowerInvariant();
        var nameHash = request.FileName.GetHashCode().ToString("X8");
        
        // Error 2: Array operations on content without null check
        logger.LogInformation("Processing file content...");
        var contentSize = request.Content.Length; // NullReferenceException if Content is null
        var firstBytes = request.Content.Take(10).ToArray(); // ArgumentNullException if Content is null
        var lastBytes = request.Content.Skip(Math.Max(0, contentSize - 10)).ToArray();
        var contentHash = request.Content.Sum(b => b) % 1000; // Will iterate through null array
        
        // Error 3: Content type processing
        logger.LogInformation("Validating content type...");
        var contentTypeParts = request.ContentType.Split('/'); // NullReferenceException if ContentType is null
        var primaryType = contentTypeParts[0]; // IndexOutOfRangeException if no slash
        var subType = contentTypeParts[1];
        
        // Error 4: File signature validation (attempts to read specific bytes)
        logger.LogInformation("Checking file signature...");
        var signature = new byte[4];
        Array.Copy(request.Content, 0, signature, 0, 4); // ArgumentException if Content is too small
        var signatureHex = BitConverter.ToString(signature).Replace("-", "");
        
        // Error 5: MIME type validation based on content
        logger.LogInformation("Performing MIME type validation...");
        var detectedMimeType = extension switch
        {
            "pdf" => request.Content[0] == 0x25 ? "application/pdf" : throw new InvalidDataException("Invalid PDF signature"),
            "jpg" => request.Content[0] == 0xFF ? "image/jpeg" : throw new InvalidDataException("Invalid JPEG signature"),
            "png" => request.Content[0] == 0x89 ? "image/png" : throw new InvalidDataException("Invalid PNG signature"),
            _ => "application/octet-stream"
        };

        var fileInfo = new
        {
            originalName = request.FileName,
            baseName = baseName,
            extension = extension,
            nameHash = nameHash,
            size = contentSize,
            contentHash = contentHash,
            contentType = request.ContentType,
            primaryType = primaryType,
            subType = subType,
            signature = signatureHex,
            detectedMimeType = detectedMimeType,
            uploadTimestamp = DateTime.UtcNow
        };

        logger.LogInformation("File upload successful: {FileName} ({FileSize} bytes)", request.FileName, contentSize);
        return Results.Ok(new { 
            message = "File uploaded successfully", 
            uploadId = Guid.NewGuid().ToString(),
            fileInfo = fileInfo
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unexpected error during file upload for {FileName}", request?.FileName ?? "unknown");
        return Results.Problem("Internal server error occurred during file upload");
    }
});

// 4. Database Operations Endpoint - Dictionary/Collection Access Errors
app.MapPost("/api/database/operation", async (DatabaseOperationRequest request, ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation("Executing database operation: {Action} for user {UserId}", request.Action, request.UserId);

        // Error 1: Dictionary operations that can throw KeyNotFoundException
        logger.LogInformation("Building SQL command from action...");
        var sqlCommands = new Dictionary<string, string>
        {
            {"CREATE", "INSERT INTO Users"},
            {"READ", "SELECT * FROM Users"},
            {"UPDATE", "UPDATE Users SET"},
            {"DELETE", "DELETE FROM Users"}
        };
        var baseCommand = sqlCommands[request.Action.ToUpperInvariant()]; // KeyNotFoundException if invalid action
        
        // Error 2: Null value arithmetic operations
        logger.LogInformation("Calculating user permissions...");
        var userPermissions = request.UserId.Value * 100; // NullReferenceException if UserId is null
        var accessLevel = userPermissions % 5;
        var canModify = accessLevel > 2;
        
        // Error 3: String operations on potentially null data
        logger.LogInformation("Processing operation data...");
        var dataJson = request.Data.Trim(); // NullReferenceException if Data is null  
        var dataLength = dataJson.Length;
        var dataChunks = dataJson.Split(','); // Will split null string
        
        // Error 4: Simulated database constraint checks using arithmetic
        logger.LogInformation("Performing constraint validation...");
        var constraintCheck = new int[] { 1, 123, 999 }; // Simulate problematic user IDs
        var constraintIndex = Array.IndexOf(constraintCheck, request.UserId); // Will work with nullable int
        
        // Simulate specific database errors based on user ID patterns
        if (request.UserId == 999)
        {
            // Force a timeout by accessing non-existent resource
            await Task.Delay(100);
            throw new TimeoutException("Database connection timeout");
        }
        
        if (request.UserId == 1 && request.Action.Equals("DELETE", StringComparison.OrdinalIgnoreCase))
        {
            // Simulate foreign key constraint
            var dependentRecords = new[] { "orders", "profiles", "sessions" };
            throw new InvalidOperationException($"Cannot delete user {request.UserId}: has dependencies in {string.Join(", ", dependentRecords)}");
        }
        
        if (request.UserId == 123 && request.Action.Equals("CREATE", StringComparison.OrdinalIgnoreCase))
        {
            // Simulate duplicate key
            throw new InvalidOperationException($"User with ID {request.UserId} already exists");
        }
        
        // Error 5: JSON parsing simulation on data field
        logger.LogInformation("Parsing operation data as JSON...");
        var parsedData = new Dictionary<string, object>();
        if (request.Data != null)
        {
            // Simulate JSON parsing that could fail
            var jsonPairs = dataJson.Split(',');
            foreach (var pair in jsonPairs)
            {
                var keyValue = pair.Split(':'); // IndexOutOfRangeException if no colon
                parsedData[keyValue[0].Trim()] = keyValue[1].Trim();
            }
        }

        var operationResult = new
        {
            operation = request.Action,
            userId = request.UserId,
            sqlCommand = baseCommand,
            userPermissions = userPermissions,
            accessLevel = accessLevel,
            canModify = canModify,
            dataLength = dataLength,
            parsedDataCount = parsedData.Count,
            timestamp = DateTime.UtcNow,
            transactionId = Guid.NewGuid().ToString()
        };

        logger.LogInformation("Database operation successful: {Action} completed for user {UserId}", request.Action, request.UserId);
        return Results.Ok(new { 
            message = $"Database {request.Action} operation completed successfully",
            result = operationResult
        });
    }
    catch (SqlException sqlEx)
    {
        logger.LogError(sqlEx, "SQL error during database operation {Action} for user {UserId}: {SqlError}", 
            request?.Action ?? "unknown", request?.UserId ?? 0, sqlEx.Message);
        return Results.Problem("Database error occurred", statusCode: 500);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unexpected error during database operation {Action} for user {UserId}", 
            request?.Action ?? "unknown", request?.UserId ?? 0);
        return Results.Problem("Internal server error occurred during database operation");
    }
});

// 5. External API Integration Endpoint - URI and HTTP Operation Errors
app.MapPost("/api/external/call", async (ExternalApiRequest request, IHttpClientFactory httpClientFactory, ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation("Making external API call to {Endpoint}", request.Endpoint);

        // Error 1: URI manipulation without validation
        logger.LogInformation("Processing endpoint URL...");
        var uri = new Uri(request.Endpoint); // UriFormatException if Endpoint is invalid or null
        var host = uri.Host.ToLowerInvariant(); // NullReferenceException if Host is null
        var pathSegments = uri.AbsolutePath.Split('/'); // Will split path
        var queryParams = uri.Query.Substring(1).Split('&'); // ArgumentOutOfRangeException if no query
        
        // Error 2: Parameter processing
        logger.LogInformation("Building request parameters...");
        var parameterCount = request.Parameters.Count; // NullReferenceException if Parameters is null
        var firstParam = request.Parameters.First(); // InvalidOperationException if empty
        var paramKeys = request.Parameters.Keys.ToArray(); // Will enumerate null collection
        var paramValues = request.Parameters.Values.ToArray();
        
        // Error 3: HTTP client configuration based on endpoint patterns
        logger.LogInformation("Configuring HTTP client...");
        var httpClient = httpClientFactory.CreateClient();
        
        // Simulate different error conditions based on endpoint content
        if (request.Endpoint.Contains("timeout") || request.Endpoint.Contains("slow"))
        {
            await Task.Delay(100);
            throw new TimeoutException($"HTTP request to {request.Endpoint} timed out");
        }
        
        if (request.Endpoint.Contains("rate-limited"))
        {
            throw new HttpRequestException("Rate limit exceeded (429 Too Many Requests)");
        }
        
        if (request.Endpoint.Contains("unauthorized"))
        {
            throw new UnauthorizedAccessException("Unauthorized access to external API");
        }
        
        if (request.Endpoint.Contains("unavailable"))
        {
            throw new HttpRequestException("Service unavailable (503 Service Unavailable)");
        }
        
        if (request.Endpoint.Contains("invalid-response"))
        {
            throw new InvalidDataException("Invalid response format from external API");
        }
        
        // Error 4: URL path analysis
        logger.LogInformation("Analyzing API endpoint structure...");
        var apiVersion = pathSegments[2]; // IndexOutOfRangeException if not enough segments
        var resource = pathSegments[3]; // IndexOutOfRangeException if path too short
        var endpointHash = request.Endpoint.GetHashCode();
        
        // Error 5: Response simulation based on parameters
        logger.LogInformation("Simulating API response...");
        var responseData = new Dictionary<string, object>();
        foreach (var param in request.Parameters)
        {
            var processedValue = param.Value.ToString().ToUpperInvariant(); // NullReferenceException if value is null
            responseData[param.Key] = processedValue;
        }

        var apiResult = new
        {
            endpoint = request.Endpoint,
            host = host,
            pathSegments = pathSegments,
            queryCount = queryParams.Length,
            parameterCount = parameterCount,
            apiVersion = apiVersion,
            resource = resource,
            endpointHash = endpointHash,
            processedParams = responseData,
            timestamp = DateTime.UtcNow,
            responseId = Guid.NewGuid().ToString()
        };

        logger.LogInformation("External API call successful to {Endpoint}", request.Endpoint);
        return Results.Ok(new { 
            message = "External API call completed successfully",
            result = apiResult
        });
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
