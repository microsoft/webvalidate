# Change Log

## v2.1.0 - July 2021

- bug fixes
- update nuget packages
- add support for xunit test output
- add support for json body and exact match

## v2.0.0 - June 2021

- updated to dotnet core 5.0
- added Prometheus support
- removed --summary-minutes
- removed --max-concurrent
- Requires new json format for test files
- --verbose always defaults to false

## v1.7.0 - April 2021

- Docker image moved to `ghcr.io/retaildevcrews/webvalidate`
- added deprecation warnings
- added `--webv-prefix` and `--webv-suffix`
- added `--log-format`
  - TSV  JSON  None

## v1.6.0 - October 2020

- added support for running tests against multiple servers

## v1.5.0 - October 2020

- added strict json parsing
- converted to system.text.json
- bug fixes

## v1.4.0 - October 2020

- added for any json array testing
- added --delay-start
- added --verbose-errors
- bug fixes

## v1.3.0 - August 2020

- Added environment variable substitution
- Moved to github.com/microsoft/webvalidate

## v1.2.0 - July 2020

- Added --tag option
- Added --summary-minutes
- Added ContentMediaType to POST

## v1.1.0 - June 2020

- Released as dotnet global tool
- updated ci-cd to publish to nuget
- bug fixes

## V1.0.0 - Jan 2020

- Initial release
