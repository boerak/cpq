# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layer for caching
COPY src/Cpq.Api/Cpq.Api.csproj ./Cpq.Api/
RUN dotnet restore ./Cpq.Api/Cpq.Api.csproj

# Copy the rest of the source and build
COPY src/Cpq.Api/ ./Cpq.Api/
RUN dotnet publish ./Cpq.Api/Cpq.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for health check
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# Create non-root user
RUN groupadd -r cpqapi && useradd -r -g cpqapi cpqapi

# Copy published output
COPY --from=build /app/publish .

# Set ownership
RUN chown -R cpqapi:cpqapi /app

USER cpqapi

EXPOSE 5000

ENTRYPOINT ["dotnet", "Cpq.Api.dll"]
