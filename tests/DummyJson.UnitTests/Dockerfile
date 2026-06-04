FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["tests/DummyJson.UnitTests/DummyJson.UnitTests.csproj", "tests/DummyJson.UnitTests/"]
COPY ["src/DummyJson.Application/DummyJson.Application.csproj", "src/DummyJson.Application/"]
COPY ["src/DummyJson.Domain/DummyJson.Domain.csproj", "src/DummyJson.Domain/"]
RUN dotnet restore "tests/DummyJson.UnitTests/DummyJson.UnitTests.csproj"
COPY . .
WORKDIR "/src/tests/DummyJson.UnitTests"
RUN dotnet build "./DummyJson.UnitTests.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DummyJson.UnitTests.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DummyJson.UnitTests.dll"]
