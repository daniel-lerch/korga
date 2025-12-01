FROM mcr.microsoft.com/dotnet/sdk:10.0 AS server
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY server/ChurchTools/ChurchTools.csproj ./ChurchTools/
COPY server/Mailist/Mailist.csproj ./Mailist/
RUN dotnet restore Mailist

# Copy everything else and build
COPY server ./
RUN dotnet publish -c Release -o /app/out Mailist

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Install curl for healthcheck
RUN set -x \
    && apt-get update \
    && apt-get install -y --no-install-recommends \
        curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=server /app/out .

HEALTHCHECK \
    --interval=1m \
    --timeout=10s \
    --start-period=15s \
    --start-interval=5s \
    --retries=3 \
    CMD curl --fail http://localhost:8080/healthz || exit 1

ENTRYPOINT ["dotnet", "Mailist.dll"]
