# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/AutomatedInventory.Api/*.csproj src/AutomatedInventory.Api/
COPY src/AutomatedInventory.Application/*.csproj src/AutomatedInventory.Application/
COPY src/AutomatedInventory.Domain/*.csproj src/AutomatedInventory.Domain/
COPY src/AutomatedInventory.Infrastructure/*.csproj src/AutomatedInventory.Infrastructure/

RUN dotnet restore src/AutomatedInventory.Api/AutomatedInventory.Api.csproj

COPY src/ src/
RUN dotnet publish src/AutomatedInventory.Api -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AutomatedInventory.Api.dll"]
