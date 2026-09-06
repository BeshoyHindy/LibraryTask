# Builds any of the three hosts: docker build --build-arg PROJECT=Library.Api .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG PROJECT
WORKDIR /source
COPY . .
RUN dotnet publish "src/${PROJECT}/${PROJECT}.csproj" --configuration Release --output /app

# No shell in the entrypoint: sh drops environment variables whose names hold a hyphen, and
# service discovery reads Services__library-service__grpc__0. Compose passes the assembly.
FROM mcr.microsoft.com/dotnet/aspnet:10.0
# Npgsql probes for Kerberos on connect and logs an error when the library is missing.
RUN apt-get update && apt-get install -y --no-install-recommends libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet"]
