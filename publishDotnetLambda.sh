#!/bin/bash

project_name="Lambda.$1"
project_dir="./$project_name/src/$project_name/"
project_file="$project_dir/$project_name.csproj"
echo "Project file: $project_file"

echo "Publishing $project_name ..."
if grep -q LambdaAotProperties "$project_file"; then
  echo "Project uses AOT"
  dotnet lambda package -c Release -farch arm64 --native-aot -ucfb true -pl "$project_dir" < /dev/null
  echo "Exit code: $?"
else
  echo "Project uses .NET runtime"
  dotnet lambda package -farch arm64 -pl "$project_dir" < /dev/null
  echo "Exit code: $?"
fi

echo Restore project to fix errors in Rider
dotnet restore "$project_file"
echo "Exit code: $?"
echo Restored project.
