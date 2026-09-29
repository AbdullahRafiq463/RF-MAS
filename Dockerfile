# ==============================================================================
# Multi-stage Dockerfile for RF-MAS ASP.NET Core Web API Backend (.NET 9)
# ==============================================================================

# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files first for layer caching
COPY ["backend/RFMAS.Core/RFMAS.Core.csproj", "backend/RFMAS.Core/"]
COPY ["backend/RFMAS.Infrastructure/RFMAS.Infrastructure.csproj", "backend/RFMAS.Infrastructure/"]
COPY ["simulator/RFMAS.DeviceSimulator/RFMAS.DeviceSimulator.csproj", "simulator/RFMAS.DeviceSimulator/"]
COPY ["backend/RFMAS.Api/RFMAS.Api.csproj", "backend/RFMAS.Api/"]

# Restore dependencies
RUN dotnet restore "backend/RFMAS.Api/RFMAS.Api.csproj"

# Copy the rest of the source code
COPY . .

# Build and publish release binaries
WORKDIR "/src/backend/RFMAS.Api"
RUN dotnet publish "RFMAS.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Production Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Cloud environments (Render, Railway, Fly.io, Cloud Run) standard port
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "RFMAS.Api.dll"]
