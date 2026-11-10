# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
USER $APP_UID
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /source
COPY ["Directory.Build.props", "."]
COPY ["src/InstaLike.Web/InstaLike.Web.csproj", "src/InstaLike.Web/"]
COPY ["src/InstaLike.Core/InstaLike.Core.csproj", "src/InstaLike.Core/"]
RUN dotnet restore "src/InstaLike.Web/InstaLike.Web.csproj"
COPY . .
WORKDIR "/source/src/InstaLike.Web"
RUN dotnet build "InstaLike.Web.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "InstaLike.Web.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InstaLike.Web.dll"]
