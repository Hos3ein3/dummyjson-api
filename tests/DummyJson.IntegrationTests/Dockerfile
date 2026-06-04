FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["tests/DummyJson.IntegrationTests/DummyJson.IntegrationTests.csproj", "tests/DummyJson.IntegrationTests/"]
COPY ["src/DummyJson.API/DummyJson.API.csproj", "src/DummyJson.API/"]
COPY ["src/DummyJson.Domain/DummyJson.Domain.csproj", "src/DummyJson.Domain/"]
COPY ["src/DummyJson.Application/DummyJson.Application.csproj", "src/DummyJson.Application/"]
COPY ["src/DummyJson.Infrastructure/DummyJson.Infrastructure.csproj", "src/DummyJson.Infrastructure/"]
COPY ["src/DummyJson.Persistence/DummyJson.Persistence.csproj", "src/DummyJson.Persistence/"]
RUN dotnet restore "tests/DummyJson.IntegrationTests/DummyJson.IntegrationTests.csproj"
COPY . .
WORKDIR "/src/tests/DummyJson.IntegrationTests"
RUN dotnet build "./DummyJson.IntegrationTests.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DummyJson.IntegrationTests.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DummyJson.IntegrationTests.dll"]
