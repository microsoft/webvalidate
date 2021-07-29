#!/bin/sh

# run dotnet restore
dotnet restore src/webvalidate.sln

# copy vscode files
mkdir -p .vscode && cp .vscode-template/* .vscode
