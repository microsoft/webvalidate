# Web Validate - A web request validation tool

![License](https://img.shields.io/badge/license-MIT-green.svg)
![Docker Image Build](https://github.com/microsoft/webvalidate/workflows/Docker%20Image%20Build/badge.svg)

Web Validate (WebV) is a web request validation tool that we use to run end-to-end tests and long-running smoke tests.

## WebV Quick Start

WebV is published as a dotnet package and can be installed as a dotnet global tool. WebV can also be run as a docker container. If you have dotnet core sdk installed, running as a dotnet global tool is the simplest and fastest way to run WebV.

There are many web test tools available. The two main differences with WebV are:

- Integrates into a `single pane of glass`
  - WebV publishes json logs to stdout and stderr
  - WebV publishes the /metrics endpoint for Prometheus
  - This allows you to build a single pane of glass that compares `server errors` with `client errors`
- Deep validation of arbitrary result graphs
  - WebV is primarily designed for json API testing and can perform `deep validation` on arbitrary json graphs

## Running as a dotnet global tool

Install WebV as a dotnet global tool

> WebV is already installed in GitHub Codespaces

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

## Running as a docker container

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

Use your own test files

```bash

# assuming you want to mount MyTestFiles to the containers /app/TestFiles
# this will start bash so you can verify the mount worked correctly
docker run -it --rm -v MyTestFiles:/app/TestFiles --entrypoint bash ghcr.io/cse-labs/webvalidate

# run a test against a local web server running on port 8080 using ~/webv/myTest.json
docker run -it --rm -v MyTestFiles:/app/TestFiles --net=host  ghcr.io/cse-labs/webvalidate --server localhost:8080 --files myTest.json

```

## Configuration

Web Validate uses both environment variables as well as command line options for configuration. Command flags take precedence over environment variables.

Web Validate works in two distinct modes. The default mode processes the input file(s) in sequential order one time and exits. The "run loop" mode runs in a continuous loop until stopped or for the specified duration. Some environment variables and command flags are only valid if run loop is specified and WebV will exit and display usage information. Some parameters have different default values depending on the mode of execution.

See [Command Line Parameters](./docs/CommandLineParameters.md) for more details.

## Validation Files

WebV uses validation files to define what requests should be run as a part of the testing. Each line in the file details the request and the expected results to be validated by WebV. This can be as simple as validating the returned status code is 200 or as complex as checking each returned value in a nested json object or array.

See [Validation Test Files](./docs/ValidationTestFiles.md) for more details.

## Integration with Application Monitoring

We use `WebV` and [Azure Container Instances](https://azure.microsoft.com/en-us/services/container-instances/) to run geo-distributed, tests against our Web APIs. These tests run 24 x 7 from multiple Azure regions and provide insight into network latency / health as well as service status.

By doing this, not only can we ensure against a [large cloud bill](https://hackernoon.com/how-we-spent-30k-usd-in-firebase-in-less-than-72-hours-307490bd24d), but we can track how cloud usage changes over time and ensure application functionality and performance through integration and load testing.

`Azure Container Instances` integrate with [Azure Monitor](https://azure.microsoft.com/en-us/services/monitor/) to provide out-of-the-box monitoring, dashboards and alerts. Setup instructions, sample queries and sample dashboards are available [here](https://github.com/retaildevcrews/helium/blob/main/docs/AppService.md#smoke-test-setup).

We use the `--log-format json` command line option to integrate Docker container logs with `Azure Log Analytics`. The integration is automatic using `Azure Container Instances`.

### Example Arguments for Long Running Tests

```bash
# continuously send a request every 15 seconds
# user defined region, tag and zone to distinguish between WebV instances

--run-loop --sleep 15000 --log-format json --tag my_webv_instance_name --region Central --zone az-central-us

```

### Example Arguments for Load Testing

```bash

# continuously run testing for 60 seconds
# default sleep between each request is 1000ms
# write all results to console as json

--run-loop --verbose --duration 60 --log-format Json

# continuously run twice as many tests against microsoft.com
# run testing for 60 seconds
--run-loop --verbose --duration 60 --sleep 500

```

### Example Dashboard

![alt text](./images/dashboard.jpg "WebV Example Dashboard")

## Running as part of an CI-CD pipeline

WebV will return a non-zero exit code (fail) under the following conditions

- Error parsing the test files
- If an unhandled exception is thrown during a test
- StatusCode validation fails
- ContentType validation fails
- --max-errors is exceeded
  - To cause the test to fail on any validation error, set --max-errors 1 (default is 10)
- Any validation error on a test that has FailOnValidationError set to true
- Request timeout

## Deprecation Warnings

> Breaking changes in v2.3.0

- The Docker repo is `ghcr.io/cse-labs/webvalidate`
- This release requires `dotnet 6.0`
  - Use WebV 2.2 for dotnet 5 support

> Breaking changes in v2.0

- The Docker repo is `ghcr.io/cse-labs/webvalidate`
- This release requires `dotnet 5.0`

## Contributing

This project welcomes contributions and suggestions. Most contributions require you to agree to a Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us the rights to use your contribution. For details, visit [Microsoft Contributor License Agreement](https://cla.opensource.microsoft.com).

When you submit a pull request, a CLA bot will automatically determine whether you need to provide a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services.

Authorized use of Microsoft trademarks or logos is subject to and must follow [Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).

Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.

Any use of third-party trademarks or logos are subject to those third-party's policies.
