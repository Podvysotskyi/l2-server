# syntax=docker/dockerfile:1.7
FROM mcr.microsoft.com/dotnet/sdk:10.0.102 AS restore

WORKDIR /workspace
COPY global.json Directory.Build.props Directory.Packages.props NuGet.Config L2.Server.slnx ./
COPY src/ src/
COPY tests/ tests/
RUN dotnet restore L2.Server.slnx

FROM restore AS build

RUN dotnet build L2.Server.slnx --configuration Release --no-restore -m:1

FROM build AS api-publish

RUN dotnet publish src/L2.Server.Api/L2.Server.Api.csproj \
    --configuration Release --no-restore --output /app/publish --property:UseAppHost=false

FROM build AS game-publish

RUN dotnet publish src/L2.Server.Game/L2.Server.Game.csproj \
    --configuration Release --no-restore --output /app/publish --property:UseAppHost=false

FROM build AS validate

RUN dotnet test L2.Server.slnx --configuration Release --no-build -m:1

FROM mcr.microsoft.com/dotnet/aspnet:10.0.2 AS api-production

WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production
COPY --from=api-publish --chown=$APP_UID:$APP_UID /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "L2.Server.Api.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0.2 AS game-production

WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production
COPY --from=game-publish --chown=$APP_UID:$APP_UID /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "L2.Server.Game.dll"]
