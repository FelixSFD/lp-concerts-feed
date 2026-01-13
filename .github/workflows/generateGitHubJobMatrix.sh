#!/bin/bash

# Define the pattern for directories to search
PATTERN="./Lambda.*/src/Lambda.*"

# make an array for project names
declare -a lambda_names=()

# Use `find` to locate directories matching the pattern and loop over them
while read -r dir; do
  echo "Processing directory: $dir"
  
  # Extract name of the lambda
  project_name="${dir##*/}"
  echo "Project name: $project_name"
  lambda_name="${project_name#Lambda.}"
  echo "Short name: $lambda_name"
  lambda_names+=("$lambda_name")

done < <(find . -type d -path "$PATTERN" -maxdepth 3)

# write data to runner variables
printf -v joined_lambda_names '"%s", ' "${lambda_names[@]}"

echo "lambda_names=[${joined_lambda_names%, }]" >> "$GITHUB_OUTPUT"