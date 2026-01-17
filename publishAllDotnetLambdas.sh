#!/bin/bash

# Define the pattern for directories to search
PATTERN="./Lambda.*/src/Lambda.*"

# Use `find` to locate directories matching the pattern and loop over them
while read -r dir; do
  echo "Processing directory: $dir"
  
  # Extract name of the lambda
  project_name="${dir##*/}"
  echo "Project name: $project_name"
  lambda_name="${project_name#Lambda.}"
  echo "Short name: $lambda_name"
  
  # Call the script that packaged that lambda
  ./publishDotnetLambda.sh "$lambda_name" || echo "Failed: $dir"
  
  echo "Published: $lambda_name"
done < <(find . -type d -path "$PATTERN" -maxdepth 3)