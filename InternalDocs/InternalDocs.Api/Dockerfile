# Multi-stage build for the InternalDocs .NET API.
# Build context is the InternalDocs/ solution folder (see docker-compose.yml).

# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first (cached unless a .csproj changes).
COPY InternalDocs.Api/InternalDocs.Api.csproj InternalDocs.Api/
COPY InternalDocs.Application/InternalDocs.Application.csproj InternalDocs.Application/
COPY InternalDocs.Infrastructure/InternalDocs.Infrastructure.csproj InternalDocs.Infrastructure/
COPY InternalDocs.Domain/InternalDocs.Domain.csproj InternalDocs.Domain/
RUN dotnet restore InternalDocs.Api/InternalDocs.Api.csproj

# Copy the rest of the sources and publish.
COPY InternalDocs.Api/ InternalDocs.Api/
COPY InternalDocs.Application/ InternalDocs.Application/
COPY InternalDocs.Infrastructure/ InternalDocs.Infrastructure/
COPY InternalDocs.Domain/ InternalDocs.Domain/
RUN dotnet publish InternalDocs.Api/InternalDocs.Api.csproj \
    -c Release -o /app --no-restore

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# curl is used by the compose healthcheck.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app ./

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "InternalDocs.Api.dll"]
