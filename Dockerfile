### build the app
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

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
FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS runtime

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
    rm -f appsettings.json && \
    rm -f stylecop.json && \
    chown -R webv:webv /app

WORKDIR /app/TestFiles

# run as the webv user
USER webv

ENTRYPOINT [ "dotnet",  "../webvalidate.dll" ]
