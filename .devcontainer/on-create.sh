#!/bin/sh

echo "on-create start" >> $HOME/status

# run dotnet restore
dotnet restore src/webvalidate.sln

# pull base docker images
docker pull mcr.microsoft.com/dotnet/aspnet:6.0-alpine
docker pull mcr.microsoft.com/dotnet/sdk:6.0

echo "on-create complete" >> $HOME/status
