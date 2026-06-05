FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY *.sln ./
COPY src/DummyJson.API/DummyJson.API.csproj src/DummyJson.API/
COPY src/DummyJson.Application/DummyJson.Application.csproj src/DummyJson.Application/
COPY src/DummyJson.Domain/DummyJson.Domain.csproj src/DummyJson.Domain/
COPY src/DummyJson.Infrastructure/DummyJson.Infrastructure.csproj src/DummyJson.Infrastructure/
COPY src/DummyJson.Persistence/DummyJson.Persistence.csproj src/DummyJson.Persistence/

RUN dotnet restore

COPY . .

RUN dotnet publish src/DummyJson.API/DummyJson.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "DummyJson.API.dll"]