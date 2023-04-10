#!/bin/bash

# Parse command-line arguments
if [ $# -ne 1 ]; then
  echo "Usage: $0 backend-port"
  exit 1
fi
BACKEND_PORT=$1

# Function to start the miner process
start_backend() {
  cd backend/DCRApi
  dotnet run --no-build $BACKEND_PORT &
  cd ../..
}

# Function to stop the miner process
stop_servers() {
  echo "Stopping miner process..."
  pkill -P $$ # kill child processes (servers)
  echo "Servers stopped."
}

# Start the miner
start_backend

# Wait for the user to press Ctrl+C
trap stop_servers SIGINT

# Wait for child processes to finish
wait