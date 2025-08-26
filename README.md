# Log Simulation API

A .NET Core 9.0 minimal API application designed to simulate realistic error scenarios for log analysis and AI-assisted debugging. This project generates comprehensive log files with various types of errors that commonly occur in real-world applications.

## 🎯 Project Purpose

This application is designed to:
- Generate realistic error logs for AI analysis
- Simulate common real-world error scenarios
- Provide structured logging for error pattern detection
- Help train AI models to propose fixes for common issues

## 🔧 Error Scenarios Covered

The API includes 5 different endpoints that simulate realistic error conditions:

### 1. User Registration (`/api/users/register`)
- **Missing required fields** (name, email, password)
- **Invalid email formats**
- **Weak password validation**
- **Age restrictions and invalid birth dates**
- **Future birth dates**

### 2. Date Processing (`/api/dates/process`)
- **Invalid date formats**
- **Date parsing failures**
- **Out-of-range dates** (too old/too future)
- **Missing date strings or formats**
- **Malformed date inputs**

### 3. File Upload (`/api/files/upload`)
- **File size validation** (5MB limit)
- **Unsupported file types**
- **Missing file extensions**
- **Corrupted file detection** (PDF signature validation)
- **Empty file content**
- **Missing content types**

### 4. Database Operations (`/api/database/operation`)
- **Connection timeouts**
- **Foreign key constraint violations**
- **Duplicate key errors**
- **Invalid user IDs**
- **Missing required data for operations**
- **User not found scenarios**

### 5. External API Integration (`/api/external/call`)
- **Network timeouts**
- **Rate limiting (429 errors)**
- **Invalid URL formats**
- **Service unavailable (503 errors)**
- **Unauthorized access (401 errors)**
- **Invalid API responses**

## 🚀 Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Python 3.7+](https://python.org) (for testing script)
- Terminal/Command Prompt

### Setup Instructions

1. **Clone or download the project files**
   
2. **Install .NET dependencies**
   ```bash
   dotnet restore
   ```

3. **Install Python dependencies**
   ```bash
   pip install -r requirements.txt
   ```

### Running the Application

1. **Start the .NET API**
   ```bash
   dotnet run
   ```
   
   The API will start on `http://localhost:5000` by default.

2. **Run the test script** (in a new terminal)
   ```bash
   python test_api.py
   ```

## 📊 Log Output

The application generates structured logs in the `logs/` directory with the following format:

```
logs/
├── app-20231225.log
├── app-20231226.log
└── ...
```

### Log Format

```
2023-12-25 14:30:15.123 +00:00 [ERR] User registration failed: Name is required but was null or empty {"RequestId":"...", "UserId":null}
2023-12-25 14:30:16.456 +00:00 [ERR] Date parsing failed: Could not parse '2023-13-01' with format 'yyyy-MM-dd' {"RequestId":"...", "DateString":"2023-13-01"}
```

## 🔍 API Endpoints

### Health Check
```
GET /health
```

### User Registration
```
POST /api/users/register
Content-Type: application/json

{
  "name": "John Doe",
  "email": "john@example.com",
  "password": "securepassword123",
  "birthDate": "1990-05-15T00:00:00Z"
}
```

### Date Processing
```
POST /api/dates/process
Content-Type: application/json

{
  "dateString": "2023-12-25",
  "format": "yyyy-MM-dd"
}
```

### File Upload
```
POST /api/files/upload
Content-Type: application/json

{
  "fileName": "document.txt",
  "content": [72, 101, 108, 108, 111],
  "contentType": "text/plain"
}
```

### Database Operation
```
POST /api/database/operation
Content-Type: application/json

{
  "userId": 123,
  "action": "CREATE",
  "data": "user data"
}
```

### External API Call
```
POST /api/external/call
Content-Type: application/json

{
  "endpoint": "https://api.example.com/users",
  "parameters": {"limit": 10}
}
```

## 🧪 Test Script Features

The Python test script (`test_api.py`) includes:

- **Comprehensive test cases** for all error scenarios
- **Both valid and invalid requests** to generate diverse logs
- **Detailed logging** of requests and responses
- **Realistic test data** that mimics real-world usage
- **Error handling** for network issues

### Test Script Output

The script provides detailed output for each test case:

```
🧪 Testing User Registration Endpoint...
============================================================
Endpoint: POST /api/users/register (Case 1)
Payload: {
  "name": "John Doe",
  "email": "john.doe@example.com",
  "password": "securepassword123",
  "birthDate": "1990-05-15T00:00:00Z"
}
Status Code: 200
Response: {"message":"User registered successfully","userId":1234}
============================================================
```

## 🎯 Using Generated Logs for AI Analysis

The generated logs are structured to be easily analyzed by AI systems for:

1. **Pattern Recognition**: Common error patterns and their contexts
2. **Root Cause Analysis**: Understanding why errors occur
3. **Fix Suggestions**: Generating appropriate solutions
4. **Code Quality Assessment**: Identifying areas for improvement

### Log Analysis Tips

- Look for repeated error patterns in the timestamp sequences
- Analyze the correlation between request data and error types
- Pay attention to the structured data in the log entries
- Use the error codes for categorizing different failure types

## 🔧 Customization

### Adding New Error Scenarios

To add new error scenarios:

1. Create a new endpoint in `Program.cs`
2. Add appropriate validation and error logging
3. Update the Python test script with new test cases
4. Document the new scenarios in this README

### Modifying Log Format

The logging is configured using Serilog. To modify the format, update the `LoggerConfiguration` in `Program.cs`:

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.File("logs/app-.log", 
        outputTemplate: "Your custom format here")
    .CreateLogger();
```

## 📝 Error Codes Reference

| Code | Description | Endpoint |
|------|-------------|----------|
| `MISSING_NAME` | Name field is required | User Registration |
| `INVALID_EMAIL_FORMAT` | Email format is invalid | User Registration |
| `WEAK_PASSWORD` | Password doesn't meet requirements | User Registration |
| `INVALID_DATE_FORMAT` | Date cannot be parsed with given format | Date Processing |
| `FILE_TOO_LARGE` | File exceeds size limit | File Upload |
| `UNSUPPORTED_FILE_TYPE` | File type not allowed | File Upload |
| `FOREIGN_KEY_CONSTRAINT` | Database constraint violation | Database Operations |
| `RATE_LIMIT_EXCEEDED` | API rate limit hit | External API |
| `USER_NOT_FOUND` | User ID doesn't exist | Database Operations |

## 🤝 Contributing

To contribute to this project:

1. Add new realistic error scenarios
2. Improve logging structure
3. Enhance test coverage
4. Add documentation for new features

## 📄 License

This project is intended for educational and testing purposes.
