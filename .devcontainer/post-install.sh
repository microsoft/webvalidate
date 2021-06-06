#!/bin/sh

# run dotnet restore
dotnet restore src/webvalidate.sln

# copy vscode files
mkdir -p .vscode && cp docs/vscode-template/* .vscode

# install / update utilities
DEBIAN_FRONTEND=noninteractive
sudo apt-get update
sudo apt-get install -y --no-install-recommends apt-utils dialog
sudo apt-get install -y --no-install-recommends dnsutils httpie bash-completion curl wget git unzip
DEBIAN_FRONTEND=dialog
