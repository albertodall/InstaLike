# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
USER $APP_UID
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["InstaLike.Web/InstaLike.Web.csproj", "InstaLike.Web/"]
COPY ["InstaLike.Core/InstaLike.Core.csproj", "InstaLike.Core/"]
RUN dotnet restore "InstaLike.Web/InstaLike.Web.csproj"
COPY . .
WORKDIR "/src/InstaLike.Web"
RUN dotnet build "InstaLike.Web.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "InstaLike.Web.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InstaLike.Web.dll"]
