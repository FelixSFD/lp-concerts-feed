FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

# install curl in the runtime image
RUN apt update \
    && apt install -y curl
RUN apt clean

USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Server.Api/Server.Api.csproj", "Server.Api/"]
RUN dotnet restore "Server.Api/Server.Api.csproj"
COPY . .
WORKDIR "/src/Server.Api"
RUN dotnet build "./Server.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Server.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
HEALTHCHECK --start-period=5s --timeout=10s --interval=15s \
 CMD curl -o - --fail http://localhost:8080/health || exit 1
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Server.Api.dll"]
