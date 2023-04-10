#!/bin/bash

# Parse command-line arguments
if [ $# -ne 2 ]; then
  echo "Usage: $0 frontend-port backend-port"
  exit 1
fi
FRONTEND_PORT=$1
BACKEND_PORT=$2

# Function to start the frontend server
start_frontend() {
  cd frontend
  VUE_APP_BACKEND=$BACKEND_PORT npm run serve -- --port $FRONTEND_PORT &
  cd ..
}

# Function to start the backend server
start_backend() {
  cd backend/DCRApi
  dotnet run --no-build $FRONTEND_PORT $BACKEND_PORT &
  cd ../..
}

# Function to stop the servers and perform cleanup
stop_servers() {
  echo "Stopping servers..."
  pkill -P $$ # kill child processes (servers)
  echo "Servers stopped."
}

# Start the servers
start_frontend
start_backend

# Wait for the user to press Ctrl+C
trap stop_servers SIGINT

# Wait for child processes to finish
wait