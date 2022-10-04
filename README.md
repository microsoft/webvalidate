# Web Validate - A web request validation tool

![License](https://img.shields.io/badge/license-MIT-green.svg)
![CodeQL Build](https://github.com/microsoft/webvalidate/workflows/CodeQL/badge.svg)
![Docker Build](https://github.com/microsoft/webvalidate/workflows/DockerBuild/badge.svg)

Web Validate (WebV) is a web request validation tool that we use to run end-to-end tests and long-running performance and availability tests.

There are many web test tools available. The two main differences with WebV are:

- Integrates into a `single pane of glass`
  - WebV publishes json logs to stdout and stderr
  - WebV publishes the /metrics endpoint for Prometheus scraping
    - This allows you to build a single pane of glass that compares `server errors` with `client errors`
    - This also allows you to monitor applications on the edge with centralized logging which reduces edge network traffic significantly
- Deep validation of arbitrary result graphs
  - WebV is primarily designed for json API testing and can perform `deep validation` on arbitrary json graphs

- WebV is published as a nuget package and can be installed as a dotnet global tool
- WebV can also be run as a docker container
- If you have dotnet core sdk installed, running as a dotnet global tool is the simplest and fastest way to run WebV

## WebV Quickstart

> The easiest way to try WebV is to fork this repo and `Open in Codespaces`
>
> WebV is already installed in this GitHub Codespace

Install WebV as a dotnet global tool

```bash

# this allows you to execute WebV from the shell
dotnet tool install -g webvalidate

```

Run a sample validation test against `microsoft.com`

```bash

# change to a directory with WebV test files in it
pushd src/app

# run a test
webv --server https://www.microsoft.com --files msft.json --verbose

```

Run more complex tests against the GitHub API by using:

```bash

# github tests
webv --server https://api.github.com --files github.json --verbose

```

Run a test that fails validation and causes a non-zero exit code

```bash

webv --server https://www.microsoft.com --files failOnValidationError.json --verbose-errors

```

Experiment with WebV

```bash

# get help
webv --help

# change back to the root of the repo
popd

```

## WebV Quickstart (docker)

Run a sample validation test against `microsoft.com`

```bash

# run the tests from Docker
docker run -it --rm ghcr.io/cse-labs/webvalidate --server https://www.microsoft.com --files msft.json --verbose

```

Run more complex tests against the GitHub API by using:

```bash

# github tests
docker run -it --rm ghcr.io/cse-labs/webvalidate --server https://api.github.com --files github.json --verbose

```

Run a test that fails validation and causes a non-zero exit code

```bash

docker run -it --rm ghcr.io/cse-labs/webvalidate --server https://www.microsoft.com --files failOnValidationError.json --verbose-errors

```

Experiment with WebV

```bash

# get help
docker run -it --rm ghcr.io/cse-labs/webvalidate --help

```

> In the above examples, the json files are included in the docker image

Use your own test files

```bash

# assuming you want to mount the current directory to the container's /app/TestFiles
# this will start bash so you can verify the mount worked correctly
docker run -it --rm -v $(pwd):/app/TestFiles --entrypoint bash ghcr.io/cse-labs/webvalidate

# run a test against a local web server running on port 8080 using ./myTest.json
docker run -it --rm -v $(pwd):/app/TestFiles --net=host  ghcr.io/cse-labs/webvalidate --server localhost:8080 --files myTest.json

```

## Configuration

> See [Command Line Parameters](./docs/CommandLineParameters.md) for more details

- Web Validate uses both environment variables and command line options for configuration
  - Command flags take precedence over environment variables

- Web Validate works in two distinct modes
  - The default mode processes the input file(s) in sequential order one time and exits
  - The `--run-loop` mode runs in a continuous loop until stopped or for the specified duration
- Some environment variables and command flags are only valid if `--run-loop` is specified and WebV will exit and display usage information
- Some parameters have different default values depending on the mode of execution

## Validation Files

> See [Validation Test Files](./docs/ValidationTestFiles.md) for more details.

- WebV uses validation files to define what requests should be run as a part of the testing
  - Each line in the file details the request and the expected results to be validated by WebV
  - This can be as simple as validating the returned status code is 200 or as complex as checking each returned value in a nested json object or array

## Integration with Application Monitoring

> See [Application Monitoring](./docs/ApplicationMonitoring.md) for more details.

- We use `WebV` to run geo-distributed tests against our Web APIs
  - These tests run 24 x 7 from multiple regions and provide insight into network latency / health as well as service status
  - The results integrate with our `single pane of glass` via log forwarding (Fluent Bit) and metrics (Prometheus)

By doing this, not only can we ensure against a [large cloud bill](https://hackernoon.com/how-we-spent-30k-usd-in-firebase-in-less-than-72-hours-307490bd24d), but we can track how usage and performance change over time, ensuring application functionality and performance through `testing in production`.

## Running as part of an CI-CD pipeline

WebV will return a non-zero exit code (fail) under the following conditions

- Error parsing the test file(s)
- If an unhandled exception is thrown during a test
  - Please use `GitHub Issues` to report as a bug
- --max-errors is exceeded
  - To cause the test to fail on any validation error, set --max-errors 1 (default is 10)
- Any validation error on a test that has `FailOnValidationError` set to true

## Contributing

This project welcomes contributions and suggestions. Most contributions require you to agree to a Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us the rights to use your contribution. For details, visit [Microsoft Contributor License Agreement](https://cla.opensource.microsoft.com).

When you submit a pull request, a CLA bot will automatically determine whether you need to provide a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services.

Authorized use of Microsoft trademarks or logos is subject to and must follow [Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).

Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.

Any use of third-party trademarks or logos are subject to those third-party's policies.
