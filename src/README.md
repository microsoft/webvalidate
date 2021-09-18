# Web Validate - A web request validation tool

![License](https://img.shields.io/badge/license-MIT-green.svg)
![Docker Image Build](https://github.com/microsoft/webvalidate/workflows/Docker%20Image%20Build/badge.svg)

Web Validate (WebV) is a web request validation tool that we use to run end-to-end tests and long-running smoke tests.

## Deprecation Warnings

- The Docker repo is `ghcr.io/cse-labs/webvalidate`
- This release `dotnet 5.0`
- `--json-log` was removed
  - use `--log-format json` instead (starting with this release)
- `--summary-minutes` was removed
  - use some type of log to store and summarize the results
- `--max-concurrent` was removed
  - use `--sleep` and `--timeout` to control connections

- Test files require the current `json format`

```json

{
  "requests":
  [
    {"path": ...}
    {"path": ...}
  ]
}

```

## Running from source

```bash

# change to the app directory
pushd src/app

```

Run a sample validation test against `microsoft.com`

```bash

# run a test
dotnet run -- --server https://www.microsoft.com --files msft.json

```

Run more complex tests against the GitHub API by using:

```bash

# github tests
dotnet run -- -s https://api.github.com -f github.json

```

Run a test that fails validation and causes a non-zero exit code

```bash

dotnet run -- -s https://www.microsoft.com -f failOnValidationError.json

```

Experiment with WebV

```bash

# get help
dotnet run -- --help

```

Make sure to change back to the root of the repo

```bash

popd

```

## Environment Variable Substitutions

WebV can substitute environment variable values in the test file(s).

- Define the environment variable substitutions in the `Variables` json array
- Include the `${VARIABLE_NAME}` in the test file(s)
- If one or more environment variables are not set, WebV will substitute with `empty string` which could cause validation errors
- The comparison is `case sensitive`

```bash

# set the environment variables
export ROBOTS=robots.txt
export FAVICON=favicon.ico

# run the test
dotnet run -- -s https://www.microsoft.com -f envvars.json

```

> JSON sample using environment variable substitution

```json

{
  "variables": [ "ROBOTS", "FAVICON" ],
  "requests": [
    {
      "path": "/${ROBOTS}",
      "validation": { "contentType": "text/plain" }
    },
    {
      "path": "/${FAVICON}",
      "validation": { "contentType": "image/x-icon" }
    }
  ]
}

```

## CI-CD

This repo uses [GitHub Actions](/.github/workflows/dockerCI.yml) for Continuous Integration.

- CI supports pushing to Azure Container Registry or DockerHub
- The action is setup to execute on a PR or commit to `main`
  - The action does not run on commits to branches other than `main`
- The action always publishes an image with the `:beta` tag
- If you tag the repo with a version i.e. `v1.1.0` the action will also
  - Tag the image with `:1.1.0`
  - Tag the image with `:latest`
  - Note that the `v` is case sensitive (lower case)

### Pushing to DockerHub

In order to push to DockerHub, you must set the following `secrets` in your GitHub repo:

- DOCKER_REPO
- DOCKER_USER
- DOCKER_PAT
  - Personal Access Token

## Contributing

This project welcomes contributions and suggestions. Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit [Microsoft Contributor License Agreement](https://cla.opensource.microsoft.com).

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
