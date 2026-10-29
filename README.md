# Automated Inventory Server

A Clean Architecture **ASP.NET Core (.NET 8)** backend and inventory management server designed for automated stock tracking, supplier orders, and real-time inventory auditing.

## Architecture

Built following **Clean Architecture** and Domain-Driven Design (DDD) principles:

```text
AutomatedInventory.sln
├── src/
│   ├── AutomatedInventory.Domain/          # Enterprise entities, value objects & domain rules
│   ├── AutomatedInventory.Application/     # Use cases, DTOs, service interfaces & validation
│   ├── AutomatedInventory.Infrastructure/  # EF Core persistence, repositories & external services
│   └── AutomatedInventory.Api/             # ASP.NET Core REST API controllers, middleware & DI
├── tests/                                  # Unit and integration test suites
├── Dockerfile
└── docker-compose.yml
```

## Getting Started

### Run with Docker Compose
```bash
docker compose up --build -d
```

### Run Locally (.NET 8 SDK)
```bash
dotnet restore AutomatedInventory.sln
dotnet build AutomatedInventory.sln
dotnet test AutomatedInventory.sln
dotnet run --project src/AutomatedInventory.Api
```

## License
MIT License
