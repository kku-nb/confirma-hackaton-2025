@echo off

echo 🚀 Log Simulation API Test Runner
echo ================================

REM Check if .NET is installed
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ❌ .NET is not installed. Please install .NET 9.0 SDK
    pause
    exit /b 1
)

REM Check if Python is installed
python --version >nul 2>&1
if errorlevel 1 (
    echo ❌ Python is not installed. Please install Python 3.7+
    pause
    exit /b 1
)

REM Install Python dependencies
echo 📦 Installing Python dependencies...
pip install -r requirements.txt

REM Build the .NET project
echo 🔨 Building .NET project...
dotnet build
if errorlevel 1 (
    echo ❌ Build failed. Please check for compilation errors.
    pause
    exit /b 1
)

REM Start the API in background
echo 🌟 Starting .NET API...
start /b dotnet run

REM Wait for API to start
echo ⏳ Waiting for API to start...
timeout /t 5 /nobreak >nul

REM Check if API is running (simplified check)
echo ✅ API should be running on http://localhost:5000

REM Run the Python test script
echo 🧪 Running test suite...
python test_api.py

echo.
echo ✅ Test suite completed! Check the logs\ directory for generated log files.
echo 🛑 Please manually stop the API by closing its console window or pressing Ctrl+C
pause
