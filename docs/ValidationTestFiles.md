# Validation Test Files

> Validations are often nested to test JSON objects / trees
>
> We use a test file generator to build complex validations

Validation files are located in the /app/TestFiles directory and are json files that control the validation tests.

You can mount a local volume into the Docker container at /app/TestFiles to test your files against your server if you don't want to rebuild the container

- HTTP redirects are not followed
- All string comparisons are case sensitive

- path (required)
  - path to resource (do not include http or dns name)
  - valid: must begin with /
- verb
  - default: GET
  - valid: HTTP verbs
- tag
  - default: string.empty
  - tag for the request
    - this will override the --tag value for that request
- failOnValidationError (optional)
  - If true, any validation error will cause that test to fail
  - default: false
- validation (optional)
  - if not specified in test file, no validation checks will run
  - statusCode
    - required
    - http status code
    - a validation error will cause the test to fail and return a non-zero error code
    - no other validation checks are executed
    - default: 200
    - valid: 100-599
  - contentType
    - required
    - http Content-Type
    - a validation error will cause the test to fail and return a non-zero error code
    - no other validation checks are executed
    - default: application/json
    - valid: valid MIME type
  - length
    - length of content
      - cannot be combined with minLength or maxLength
    - valid: null or >= 0
  - minLength
    - minimum content length
    - valid: null or >= 0
  - maxLength
    - maximum content length
    - valid: null or > MinLength
    - valid: if MinLength == null >= 0
  - maxMilliSeconds
    - maximum duration in ms
    - valid: null or > 0
  - exactMatch
    - Body exactly matches value
    - valid: non-empty string
  - contains[string]
    - case sensitive string "contains"
    - string array
      - valid: non-empty string array
  - notContains[string]
    - case sensitive negated string "contains"
    - string array
      - valid: non-empty string array
  - jsonArray
    - valid: parses into json array
    - count
      - exact number of items
      - Valid: >= 0
      - valid: cannot be combined with MinCount or MaxCount
    - minCount
      - minimum number of items
      - valid: >= 0
        - can be combined with MaxCount
    - maxCount
      - maximum number of items
      - valid: > MinCount
        - can be combined with MinCount
    - byIndex[JsonObject]
      - checks a json object in the array by index
      - jsonObject[]
        - validates object[index]
        - index
          - index of object to check
          - valid: >= 0
        - jsonObject
          - jsonObject definition to check
          - valid: JsonObject rules
    - forAny[JsonObject]
      - checks each json object in the array until it finds a valid item
      - jsonObject[]
        - jsonObject
          - jsonObject definition to check
          - valid: jsonObject rules
    - forEach[JsonObject]
      - checks each json object in the array
      - jsonObject[]
        - jsonObject
          - jsonObject definition to check
          - valid: jsonObject rules
  - jsonObject[]
    - valid: parses into json object
    - field
      - name of field
      - valid: non-empty string
    - value (optional)
      - if not specified, verifies the Field exists in the json document
      - valid: null, number or string
    - validation (optional)
      - validation object to execute (for json objects within objects)
      - valid: null or valid json
- perfTarget (optional)
  - category
    - used to group requests into categories for reporting
    - valid: non-empty string
  - targets[3]
    - maximum quartile value in ascending order
    - example: [ 100, 200, 400 ]
      - Quartile 1 <= 100 ms
      - Quartile 2 <= 200 ms
      - Quartile 3 <= 400 ms
      - Quartile 4 > 400 ms

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
webv -s https://www.microsoft.com --files envvars.json

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

## Sample `microsoft.com` validation tests

The msft.json file contains sample validation tests that will will successfully run against the `microsoft.com` endpoint (assuming content hasn't changed)

- note that http status codes are not specified when 200 is expected
- note that ContentType is not specified when the default of application/json is expected

### Redirect from home page

- Note that redirects are not followed

```json

{
  "path":"/",
  "validation": { "statusCode":302 }
}

```

### home page (en-us)

```json

{
  "path":"/en-us",
  "validation":
  {"
    contentType":"text/html",
    "contains":
    [
      { "value":"<title>Microsoft - Official Home Page</title>" },
      { "value":"<head data-info=\"{" }
    ]
  }
}

```

### favicon

```json

{
  "path": "/favicon.ico",
  "validation":
  {
    "contentType":"image/x-icon"
  }
}

```

### robots.txt

```json

{
  "path": "/robots.txt",
  "validation":
  {
    "contentType": "text/plain",
    "minLength": 200,
    "contains":
    [
      { "value": "User-agent: *" },
      { "value": "Disallow: /en-us/windows/si/matrix.html"}
    ]
  }
}

```

## Sample GitHub tests

### Array of Repositories

```json

{
  "path": "/orgs/octokit/repos",
  "validation": {
    "contentType": "application/json",
    "jsonArray": {
      "count": 30,
      "forEach": [
        {
          "jsonObject": [
            { "field": "id" },
            { "field": "node_id" },
            { "field": "name" },
            { "field": "full_name" },
            { "field": "private" },
            {
              "field": "owner",
              "validation": {
                "jsonObject": [
                  { "field": "login" },
                  { "field": "id" },
                  { "field": "node_id" },
                  { "field": "avatar_url" },
                  { "field": "gravatar_id" },
                  { "field": "url" },
                  { "field": "html_url" },
                  { "field": "followers_url" },
                  { "field": "following_url" },
                  { "field": "gists_url" },
                  { "field": "starred_url" },
                  { "field": "subscriptions_url" },
                  { "field": "organizations_url" },
                  { "field": "repos_url" },
                  { "field": "events_url" },
                  { "field": "received_events_url" },
                  { "field": "type" },
                  { "field": "site_admin" }
                ]
              }
            }
          ]
        }
      ]
    }
  }
}

```

### Single Repository Validation

```json

{
  "path": "/repos/octokit/octokit.net",
  "validation": {
    "contentType": "application/json",
    "jsonObject": [
      {
        "field": "id",
        "value": 7528679
      },
      {
        "field": "name",
        "value": "octokit.net"
      },
      {
        "field": "owner",
        "validation": {
          "jsonObject": [
            {
              "field": "login",
              "value": "octokit"
            },
            {
              "field": "id",
              "value": 3430433
            },
            {
              "field": "url",
              "value": "https://api.github.com/users/octokit"
            },
            {
              "field": "html_url",
              "value": "https://github.com/octokit"
            },
            {
              "field": "type",
              "value": "Organization"
            }
          ]
        }
      },
      {
        "field": "html_url",
        "value": "https://github.com/octokit/octokit.net"
      }
    ]
  }
}

```
