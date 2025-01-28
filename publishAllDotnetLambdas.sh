#!/bin/bash

# Define the pattern for directories to search
PATTERN="./Lambda.*/src/Lambda.*"


# Use `find` to locate directories matching the pattern and loop over them
find . -type d -path "$PATTERN" -maxdepth 3 | while read -r dir; do
  echo "Processing directory: $dir"
  # Navigate to the directory
  cd "$dir" || { echo "Failed to enter directory: $dir"; continue; }
  # Run the dotnet lambda package command
  dotnet lambda package -farch arm64
  # Return to the previous directory
  cd - > /dev/null || exit
done
