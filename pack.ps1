#!/usr/bin/env bash

# Clean and build in release
dotnet restore
dotnet clean
dotnet build -c Release

# Create all NuGet packages

dotnet pack src/Indice.IdentityServer4/Indice.IdentityServer4.csproj --no-build -c Release -o ./artifacts
dotnet pack src/Indice.IdentityServer4.EntityFramework.Storage/Indice.IdentityServer4.EntityFramework.Storage.csproj --no-build -c Release -o ./artifacts