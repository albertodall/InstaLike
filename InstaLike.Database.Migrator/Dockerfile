FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["InstaLike.Database.Migrator/InstaLike.Database.Migrator.csproj", "InstaLike.Database.Migrator/"]
RUN dotnet restore "InstaLike.Database.Migrator/InstaLike.Database.Migrator.csproj"
COPY . .
WORKDIR "/src/InstaLike.Database.Migrator"
RUN dotnet publish "InstaLike.Database.Migrator.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "InstaLike.Database.Migrator.dll"]
