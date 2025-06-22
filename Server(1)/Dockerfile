# Multi-stage build for production
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install required packages for PostgreSQL and image processing
RUN apt-get update && apt-get install -y \
    libgdiplus \
    libc6-dev \
    curl \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Server(1).csproj", "."]
RUN dotnet restore "Server(1).csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "Server(1).csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Server(1).csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create non-root user for security
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "Server(1).dll"]
