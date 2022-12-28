#!/usr/bin/env bash

# Clean and build in release
dotnet restore
dotnet clean
dotnet build -c Release

# Create all NuGet packages
dotnet pack src/Indice.IdentityServer/Indice.IdentityServer.csproj --no-build -c Release -o ./artifacts
dotnet pack src/Indice.IdentityServer.EntityFramework.Storage/Indice.IdentityServer.EntityFramework.Storage.csproj --no-build -c Release -o ./artifacts