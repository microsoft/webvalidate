### build the app
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build

# Copy the source
COPY src /src

### Run the unit tests
WORKDIR /src/tests
RUN dotnet test

### Build the release app
WORKDIR /src/app
RUN dotnet publish -c Release -o /app

    
###########################################################


### build the runtime container
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine AS runtime

### create a user
### dotnet needs a home directory
RUN addgroup -S webv && \
    adduser -S webv -G webv && \
    mkdir -p /home/webv && \
    chown -R webv:webv /home/webv

WORKDIR /app
COPY --from=build /app .
RUN mkdir -p /app/TestFiles && \
    cp *.json TestFiles && \
    cp perfTargets.txt TestFiles && \
    chown -R webv:webv /app

WORKDIR /app/TestFiles

# run as the webv user
USER webv

ENTRYPOINT [ "dotnet",  "../webvalidate.dll" ]
