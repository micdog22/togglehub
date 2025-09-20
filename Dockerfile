
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/ToggleHub.Core/ToggleHub.Core.csproj src/ToggleHub.Core/
COPY src/ToggleHub.Api/ToggleHub.Api.csproj src/ToggleHub.Api/
COPY tests/ToggleHub.Tests/ToggleHub.Tests.csproj tests/ToggleHub.Tests/
RUN dotnet restore src/ToggleHub.Api/ToggleHub.Api.csproj
COPY . .
RUN dotnet build src/ToggleHub.Api/ToggleHub.Api.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish src/ToggleHub.Api/ToggleHub.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "ToggleHub.Api.dll"]
