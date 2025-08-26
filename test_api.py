#!/usr/bin/env python3
"""
Test script for the Log Simulation API
This script makes requests to all endpoints with both valid and invalid data
to generate realistic error logs for AI analysis.
"""

import requests
import json
import time
import random
import base64
from datetime import datetime, timedelta
from typing import Dict, List, Any

BASE_URL = "http://localhost:5000"

def log_request(endpoint: str, payload: Dict[str, Any], response: requests.Response):
    """Log the request and response details"""
    print(f"\n{'='*60}")
    print(f"Endpoint: {endpoint}")
    print(f"Payload: {json.dumps(payload, indent=2, default=str)}")
    print(f"Status Code: {response.status_code}")
    print(f"Response: {response.text}")
    print(f"{'='*60}")

def test_user_registration():
    """Test user registration endpoint with natural exception scenarios"""
    print("\n🧪 Testing User Registration Endpoint...")
    
    test_cases = [
        # Valid case - all fields properly filled
        {
            "name": "John Michael Doe",
            "email": "john.doe@example.com",
            "password": "SecurePass123",
            "birthDate": "1990-05-15T00:00:00Z"
        },
        # Error 1: NullReferenceException - null name (triggers name.Trim())
        {
            "name": None,
            "email": "jane@example.com",
            "password": "password123",
            "birthDate": "1985-03-20T00:00:00Z"
        },
        # Error 2: IndexOutOfRangeException - name without space (triggers name.Split(' ')[1])
        {
            "name": "SingleName",
            "email": "single@example.com",
            "password": "password123",
            "birthDate": "1988-07-10T00:00:00Z"
        },
        # Error 3: NullReferenceException - null email (triggers email.Split('@'))
        {
            "name": "Alice Johnson",
            "email": None,
            "password": "password123",
            "birthDate": "1992-12-01T00:00:00Z"
        },
        # Error 4: IndexOutOfRangeException - email without @ (triggers emailParts[1])
        {
            "name": "Bob Smith",
            "email": "invalid-email-no-at",
            "password": "password123",
            "birthDate": "1988-07-10T00:00:00Z"
        },
        # Error 5: NullReferenceException - null password (triggers password.Any())
        {
            "name": "Charlie Brown",
            "email": "charlie@example.com",
            "password": None,
            "birthDate": "1995-03-15T00:00:00Z"
        },
        # Error 6: NullReferenceException - null birthDate (triggers birthDate.Value)
        {
            "name": "Diana Prince",
            "email": "diana@example.com",
            "password": "WonderWoman123",
            "birthDate": None
        },
        # Valid case with different structure
        {
            "name": "Emma Watson Smith",
            "email": "emma.watson@hogwarts.edu",
            "password": "Hermione2024!",
            "birthDate": "1995-04-15T00:00:00Z"
        },
        # Error 7: FormatException - domain without dot
        {
            "name": "Frank Miller",
            "email": "frank@nodotdomain",
            "password": "FrankPass123",
            "birthDate": "1980-01-01T00:00:00Z"
        }
    ]
    
    for i, case in enumerate(test_cases):
        try:
            response = requests.post(f"{BASE_URL}/api/users/register", json=case)
            log_request(f"POST /api/users/register (Case {i+1})", case, response)
            time.sleep(0.5)  # Small delay between requests
        except Exception as e:
            print(f"Error making request {i+1}: {e}")

def test_date_processing():
    """Test date processing endpoint with natural format exceptions"""
    print("\n📅 Testing Date Processing Endpoint...")
    
    test_cases = [
        # Valid cases
        {"dateString": "2023-12-25", "format": "yyyy-MM-dd"},
        {"dateString": "25/12/2023", "format": "dd/MM/yyyy"},
        {"dateString": "December 25, 2023", "format": "MMMM dd, yyyy"},
        
        # Error 1: NullReferenceException - null dateString (triggers dateString.Trim())
        {"dateString": None, "format": "yyyy-MM-dd"},
        
        # Error 2: NullReferenceException - null format (triggers format.Split())
        {"dateString": "2023-12-25", "format": None},
        
        # Error 3: IndexOutOfRangeException - format without separators (triggers formatParts[0])
        {"dateString": "2023-12-25", "format": ""},
        
        # Error 4: FormatException - invalid date format for DateTime.ParseExact
        {"dateString": "2023-13-45", "format": "yyyy-MM-dd"},  # Invalid date
        {"dateString": "not-a-date", "format": "yyyy-MM-dd"},  # Non-numeric
        {"dateString": "2023-12-25", "format": "invalid-format"},  # Bad format
        
        # Error 5: FormatException - wrong format combinations
        {"dateString": "25/12/2023", "format": "yyyy-MM-dd"},  # Format mismatch
        {"dateString": "2023-12-25", "format": "dd/MM/yyyy"},  # Format mismatch
        
        # Error 6: FormatException - invalid custom format in ToString()
        {"dateString": "2023-12-25", "format": "invalid{format}"},
        {"dateString": "2023-12-25", "format": "bad%%%format"},
        
        # Valid edge cases
        {"dateString": "2024-02-29", "format": "yyyy-MM-dd"},  # Leap year
        {"dateString": "01/01/2000", "format": "MM/dd/yyyy"},  # Y2K date
        
        # Error 7: More format mismatches
        {"dateString": "25th December 2023", "format": "yyyy-MM-dd"},
        {"dateString": "2023.12.25", "format": "yyyy-MM-dd"},  # Wrong separator
    ]
    
    for i, case in enumerate(test_cases):
        try:
            response = requests.post(f"{BASE_URL}/api/dates/process", json=case)
            log_request(f"POST /api/dates/process (Case {i+1})", case, response)
            time.sleep(0.5)
        except Exception as e:
            print(f"Error making request {i+1}: {e}")

def test_file_upload():
    """Test file upload endpoint with array/collection operation errors"""
    print("\n📁 Testing File Upload Endpoint...")
    
    # Create sample file contents
    text_content = b"This is a sample text file content for testing."
    pdf_content = b"%PDF-1.4\n1 0 obj<</Type/Catalog/Pages 2 0 R>>endobj"
    jpg_content = b"\xff\xd8\xff\xe0" + b"fake jpeg data"  # JPEG signature + data
    png_content = b"\x89PNG\r\n\x1a\n" + b"fake png data"  # PNG signature + data
    small_content = b"ab"  # Too small for 4-byte signature check
    
    test_cases = [
        # Valid cases
        {
            "fileName": "document.txt",
            "content": list(text_content),
            "contentType": "text/plain"
        },
        {
            "fileName": "sample.pdf",
            "content": list(pdf_content),
            "contentType": "application/pdf"
        },
        {
            "fileName": "image.jpg",
            "content": list(jpg_content),
            "contentType": "image/jpeg"
        },
        
        # Error 1: NullReferenceException - null fileName (triggers fileName.Split())
        {
            "fileName": None,
            "content": list(text_content),
            "contentType": "text/plain"
        },
        
        # Error 2: IndexOutOfRangeException - fileName without extension (triggers fileNameParts[0])
        {
            "fileName": "noextension",
            "content": list(text_content),
            "contentType": "text/plain"
        },
        
        # Error 3: NullReferenceException - null content (triggers content.Length)
        {
            "fileName": "empty.txt",
            "content": None,
            "contentType": "text/plain"
        },
        
        # Error 4: NullReferenceException - null contentType (triggers contentType.Split())
        {
            "fileName": "document.txt",
            "content": list(text_content),
            "contentType": None
        },
        
        # Error 5: IndexOutOfRangeException - contentType without slash (triggers contentTypeParts[1])
        {
            "fileName": "document.txt",
            "content": list(text_content),
            "contentType": "invalidcontenttype"
        },
        
        # Error 6: ArgumentException - content too small for signature check (triggers Array.Copy with 4 bytes)
        {
            "fileName": "tiny.pdf",
            "content": list(small_content),  # Only 2 bytes
            "contentType": "application/pdf"
        },
        
        # Error 7: InvalidDataException - wrong file signature
        {
            "fileName": "fake.pdf",
            "content": list(text_content),  # Text content but PDF extension
            "contentType": "application/pdf"
        },
        {
            "fileName": "fake.jpg",
            "content": list(text_content),  # Text content but JPG extension
            "contentType": "image/jpeg"
        },
        {
            "fileName": "fake.png",
            "content": list(text_content),  # Text content but PNG extension
            "contentType": "image/png"
        },
        
        # Valid cases with proper signatures
        {
            "fileName": "photo.png",
            "content": list(png_content),
            "contentType": "image/png"
        },
        
        # Error 8: Empty content array (triggers content.Length)
        {
            "fileName": "empty.txt",
            "content": [],
            "contentType": "text/plain"
        }
    ]
    
    for i, case in enumerate(test_cases):
        try:
            response = requests.post(f"{BASE_URL}/api/files/upload", json=case)
            log_request(f"POST /api/files/upload (Case {i+1})", 
                       {**case, "content": f"<{len(case.get('content', []))} bytes>"}, response)
            time.sleep(0.5)
        except Exception as e:
            print(f"Error making request {i+1}: {e}")

def test_database_operations():
    """Test database operations endpoint with dictionary/collection access errors"""
    print("\n🗄️ Testing Database Operations Endpoint...")
    
    test_cases = [
        # Valid cases
        {"userId": 100, "action": "READ", "data": "name:John,age:30"},
        {"userId": 200, "action": "CREATE", "data": "name:Jane,email:jane@test.com"},
        {"userId": 300, "action": "UPDATE", "data": "name:Updated,status:active"},
        
        # Error 1: KeyNotFoundException - invalid action (triggers sqlCommands[action])
        {"userId": 100, "action": "INVALID", "data": "some data"},
        {"userId": 100, "action": "DESTROY", "data": "some data"},
        {"userId": 100, "action": "EXECUTE", "data": "some data"},
        
        # Error 2: NullReferenceException - null userId (triggers userId.Value)
        {"userId": None, "action": "READ", "data": "name:test"},
        
        # Error 3: NullReferenceException - null data (triggers data.Trim())
        {"userId": 400, "action": "UPDATE", "data": None},
        
        # Error 4: IndexOutOfRangeException - data without colons (triggers keyValue[1])
        {"userId": 500, "action": "CREATE", "data": "invalid_data_format"},
        {"userId": 501, "action": "UPDATE", "data": "name,email,age"},  # No colons
        
        # Specific error simulations based on userId patterns
        # TimeoutException - userId 999
        {"userId": 999, "action": "READ", "data": "name:timeout_test"},
        
        # InvalidOperationException - DELETE user 1 (foreign key constraint)
        {"userId": 1, "action": "DELETE", "data": "force:true"},
        
        # InvalidOperationException - CREATE user 123 (duplicate key)
        {"userId": 123, "action": "CREATE", "data": "name:duplicate"},
        
        # Valid edge cases
        {"userId": 50, "action": "read", "data": "name:lowercase_action"},  # Lowercase
        {"userId": 75, "action": "Delete", "data": "name:mixed_case"},      # Mixed case
        
        # Error 5: More data parsing errors
        {"userId": 600, "action": "UPDATE", "data": "name:value:extra:colons"},  # Too many colons
        {"userId": 601, "action": "CREATE", "data": ":empty_key"},               # Empty key
        {"userId": 602, "action": "UPDATE", "data": "empty_value:"},             # Empty value
        
        # Valid complex data
        {"userId": 700, "action": "CREATE", "data": "name:Alice,email:alice@test.com,role:admin"},
    ]
    
    for i, case in enumerate(test_cases):
        try:
            response = requests.post(f"{BASE_URL}/api/database/operation", json=case)
            log_request(f"POST /api/database/operation (Case {i+1})", case, response)
            time.sleep(0.5)
        except Exception as e:
            print(f"Error making request {i+1}: {e}")

def test_external_api():
    """Test external API endpoint with URI and HTTP operation errors"""
    print("\n🌐 Testing External API Endpoint...")
    
    test_cases = [
        # Valid cases
        {"endpoint": "https://api.example.com/v1/users/list", "parameters": {"limit": 10, "format": "json"}},
        {"endpoint": "https://jsonplaceholder.typicode.com/v2/posts?sort=asc", "parameters": {"page": 1}},
        
        # Error 1: UriFormatException - invalid URI (triggers new Uri())
        {"endpoint": "not-a-valid-url", "parameters": {"test": "data"}},
        {"endpoint": "invalid://bad-url-format", "parameters": {}},
        {"endpoint": "http://", "parameters": {}},
        
        # Error 2: NullReferenceException - null endpoint (triggers new Uri())
        {"endpoint": None, "parameters": {"key": "value"}},
        
        # Error 3: NullReferenceException - null parameters (triggers parameters.Count)
        {"endpoint": "https://api.example.com/v1/test", "parameters": None},
        
        # Error 4: InvalidOperationException - empty parameters (triggers parameters.First())
        {"endpoint": "https://api.example.com/v1/empty", "parameters": {}},
        
        # Error 5: IndexOutOfRangeException - insufficient path segments (triggers pathSegments[2])
        {"endpoint": "https://api.com", "parameters": {"test": "short_path"}},        # No path segments
        {"endpoint": "https://api.com/v1", "parameters": {"test": "short_path"}},     # Only 1 segment
        {"endpoint": "https://api.com/v1/users", "parameters": {"test": "data"}},    # Only 2 segments, needs 3+
        
        # Specific error trigger keywords for simulated behaviors
        # TimeoutException
        {"endpoint": "https://api.slow.com/v1/timeout/test", "parameters": {"delay": 5000}},
        
        # HttpRequestException (rate limiting)
        {"endpoint": "https://api.rate-limited.com/v1/data/fetch", "parameters": {"user": "test"}},
        
        # UnauthorizedAccessException
        {"endpoint": "https://api.unauthorized.com/v1/secure/data", "parameters": {"token": "invalid"}},
        
        # HttpRequestException (service unavailable)
        {"endpoint": "https://api.unavailable.com/v1/service/status", "parameters": {"check": "health"}},
        
        # InvalidDataException
        {"endpoint": "https://api.invalid-response.com/v1/broken/data", "parameters": {"format": "xml"}},
        
        # Error 6: NullReferenceException - null parameter values (triggers param.Value.ToString())
        {"endpoint": "https://api.example.com/v1/test/params", "parameters": {"key1": "value1", "key2": None}},
        
        # Valid edge cases
        {"endpoint": "https://api.github.com/v3/repos/owner/repo", "parameters": {"per_page": 100, "sort": "updated"}},
        {"endpoint": "https://httpbin.org/get?query=test", "parameters": {"echo": "true", "format": "json"}},
        
        # Error 7: ArgumentOutOfRangeException - URL without query (triggers uri.Query.Substring(1))
        {"endpoint": "https://api.example.com/v1/noquery", "parameters": {"test": "no_query"}},
    ]
    
    for i, case in enumerate(test_cases):
        try:
            response = requests.post(f"{BASE_URL}/api/external/call", json=case)
            log_request(f"POST /api/external/call (Case {i+1})", case, response)
            time.sleep(0.5)
        except Exception as e:
            print(f"Error making request {i+1}: {e}")

def test_health_check():
    """Test the health check endpoint"""
    print("\n❤️ Testing Health Check Endpoint...")
    try:
        response = requests.get(f"{BASE_URL}/health")
        log_request("GET /health", {}, response)
    except Exception as e:
        print(f"Error checking health: {e}")

def main():
    """Main function to run all tests"""
    print("🚀 Starting API Test Suite")
    print(f"Target URL: {BASE_URL}")
    print(f"Test started at: {datetime.now()}")
    
    # Test if API is reachable
    try:
        response = requests.get(f"{BASE_URL}/health", timeout=5)
        if response.status_code != 200:
            print(f"❌ API health check failed with status {response.status_code}")
            return
        print("✅ API is reachable")
    except Exception as e:
        print(f"❌ Cannot reach API at {BASE_URL}: {e}")
        print("Make sure the .NET API is running on the correct port!")
        return
    
    # Run all test suites
    try:
        test_health_check()
        test_user_registration()
        test_date_processing()
        test_file_upload()
        test_database_operations()
        test_external_api()
        
        print(f"\n🎉 Test suite completed at: {datetime.now()}")
        print("📋 Check the logs/ directory for generated log files")
        
    except KeyboardInterrupt:
        print("\n⏹️ Test suite interrupted by user")
    except Exception as e:
        print(f"\n💥 Unexpected error during testing: {e}")

if __name__ == "__main__":
    main()
