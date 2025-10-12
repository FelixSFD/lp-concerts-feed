#!/bin/bash

echo Publishing Lambda.$1 ...
dotnet lambda package -c Release -farch arm64 --native-aot -ucfb true -pl ./Lambda.$1/src/Lambda.$1/

echo Restore project to fix errors in Rider
dotnet restore ./Lambda.$1/src/Lambda.$1/Lambda.$1.csproj
echo Restored project.
