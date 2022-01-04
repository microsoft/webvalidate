#!/bin/bash

echo "post-start start" >> $HOME/status

# this runs each time the container starts

# run dotnet restore
dotnet restore src/webvalidate.sln

# update the base docker images
docker pull mcr.microsoft.com/dotnet/aspnet:6.0-alpine
docker pull mcr.microsoft.com/dotnet/sdk:6.0

echo "post-start complete" >> $HOME/status
