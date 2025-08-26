#!/bin/bash

# Script to run the Log Simulation API and test suite

echo "🚀 Log Simulation API Test Runner"
echo "================================"

# Check if .NET is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET is not installed. Please install .NET 9.0 SDK"
    exit 1
fi

# Check if Python is installed
if ! command -v python3 &> /dev/null; then
    echo "❌ Python 3 is not installed. Please install Python 3.7+"
    exit 1
fi

# Install Python dependencies
echo "📦 Installing Python dependencies..."
pip3 install -r requirements.txt

# Build the .NET project
echo "🔨 Building .NET project..."
dotnet build

if [ $? -ne 0 ]; then
    echo "❌ Build failed. Please check for compilation errors."
    exit 1
fi

# Start the API in background
echo "🌟 Starting .NET API..."
dotnet run &
API_PID=$!

# Wait for API to start
echo "⏳ Waiting for API to start..."
sleep 5

# Check if API is running
if ! curl -s http://localhost:5000/health > /dev/null; then
    echo "❌ API failed to start properly"
    kill $API_PID
    exit 1
fi

echo "✅ API is running on http://localhost:5000"

# Run the Python test script
echo "🧪 Running test suite..."
python3 test_api.py

# Stop the API
echo "🛑 Stopping API..."
kill $API_PID

echo "✅ Test suite completed! Check the logs/ directory for generated log files."
