FROM node:16 AS webapp
WORKDIR /app

# Copy package definition and restore node modules as distinct layers
COPY webapp/package.json webapp/package-lock.json ./
RUN npm install

# Copy everything else and build
COPY webapp ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS server
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY server/src/Korga.Server/Korga.Server.csproj ./Korga.Server/
RUN dotnet restore Korga.Server

# Copy everything else and build
COPY server/src ./
RUN dotnet publish -c Release -o /app/out Korga.Server

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=server /app/out .
COPY --from=webapp /app/dist wwwroot/
ENTRYPOINT ["dotnet", "Korga.Server.dll"]
