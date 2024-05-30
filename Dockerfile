FROM node:20 AS webapp
WORKDIR /app

# Copy package definition and restore node modules as distinct layers
COPY webapp/package.json webapp/package-lock.json ./
RUN npm install

# Copy everything else and build
COPY webapp ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS server
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY server/Korga/Korga.csproj ./Korga/
RUN dotnet restore Korga

# Copy everything else and build
COPY server ./
RUN dotnet publish -c Release -o /app/out Korga

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Install curl for healthcheck
RUN set -x \
    && apt-get update \
    && apt-get install -y --no-install-recommends \
        curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=server /app/out .
COPY --from=webapp /app/dist wwwroot/

HEALTHCHECK CMD curl --fail http://localhost:8080/healthz || exit 1

ENTRYPOINT ["dotnet", "Korga.dll"]
