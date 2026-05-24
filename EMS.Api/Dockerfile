# Multi-stage build for EMS API
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy project files
COPY ["EMS.Api/EMS.Api.csproj", "EMS.Api/"]
COPY ["EMS.Application/EMS.Application.csproj", "EMS.Application/"]
COPY ["EMS.Domain/EMS.Domain.csproj", "EMS.Domain/"]
COPY ["EMS.Infrastructure/EMS.Infrastructure.csproj", "EMS.Infrastructure/"]
COPY ["EMS.Shared/EMS.Shared.csproj", "EMS.Shared/"]

# Restore dependencies
RUN dotnet restore "EMS.Api/EMS.Api.csproj"

# Copy source code
COPY . .

# Build application
RUN dotnet build "EMS.Api/EMS.Api.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish

RUN dotnet publish "EMS.Api/EMS.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

# Copy published files
COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p /app/logs

# Set environment
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5000

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:5000/health || exit 1

# Expose port
EXPOSE 5000

# Run application
ENTRYPOINT ["dotnet", "EMS.Api.dll"]
