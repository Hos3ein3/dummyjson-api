# Use the official ASP.NET Core runtime as a base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Use the SDK image for build and publish
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy all project files and restore as distinct layers
COPY ["src/DummyJson.API/DummyJson.API.csproj", "src/DummyJson.API/"]
COPY ["src/DummyJson.Application/DummyJson.Application.csproj", "src/DummyJson.Application/"]
COPY ["src/DummyJson.Domain/DummyJson.Domain.csproj", "src/DummyJson.Domain/"]
COPY ["src/DummyJson.Infrastructure/DummyJson.Infrastructure.csproj", "src/DummyJson.Infrastructure/"]
COPY ["src/DummyJson.Persistence/DummyJson.Persistence.csproj", "src/DummyJson.Persistence/"]
COPY ["src/SharedKernel/SharedKernel.csproj", "src/SharedKernel/"]
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]

RUN dotnet restore "src/DummyJson.API/DummyJson.API.csproj"

# Copy the remaining source code and build
COPY . .
WORKDIR "/src/src/DummyJson.API"
RUN dotnet build "DummyJson.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "DummyJson.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Configure entry point
ENTRYPOINT ["dotnet", "DummyJson.API.dll"]