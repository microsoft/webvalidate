#!/bin/sh

echo "on-create start" >> ~/status

# run dotnet restore
dotnet restore src/webvalidate.sln

echo "on-create complete" >> ~/status
