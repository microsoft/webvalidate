#!/bin/sh

# run dotnet restore
dotnet restore src/webvalidate.sln

DEBIAN_FRONTEND=noninteractive
# update apt-get
sudo apt-get update
sudo apt-get install -y --no-install-recommends apt-utils dialog

# add docker bash-completion
sudo curl https://raw.githubusercontent.com/docker/docker-ce/master/components/cli/contrib/completion/bash/docker -o /etc/bash_completion.d/docker

# install / update utilities
sudo apt-get install -y --no-install-recommends dnsutils httpie bash-completion curl wget git unzip

DEBIAN_FRONTEND=dialog

# copy vscode files
mkdir -p .vscode && cp docs/vscode-template/* .vscode

# source the bashrc-append from the repo
# you can add project specific settings to .bashrc-append and
# they will be added for every user that clones the repo with Codespaces
# including keys or secrets could be a SECURITY RISK
echo "" >> ~/.bashrc
echo ". ${PWD}/.devcontainer/.bashrc-append" >> ~/.bashrc
