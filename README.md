# FairFare

FairFare is a WPF travel-planning and shared-expense application following the layered architecture described in `docs/task3.md` and `docs/task4.md`.

## Solution layout

- `src/FairFare.Domain`: domain entities and financial models
- `src/FairFare.BLL`: business logic, validation, route handling, and reporting services
- `src/FairFare.DAL`: Entity Framework Core data layer with PostgreSQL (Npgsql)
- `src/FairFare.UI`: WPF desktop application
- `src/FairFare.Tests`: xUnit tests for financial calculations

## Run

```bash
cd src
dotnet restore
dotnet build FairFare.slnx
dotnet run --project FairFare.UI/FairFare.UI.csproj
```
## Запуск

1. Встановіть .NET 10 SDK і Docker.
2. `docker compose up -d` (PostgreSQL на порту 5433)
3. `dotnet tool restore` (інструмент dotnet-ef)
4. `cd src && dotnet run --project FairFare.UI/FairFare.UI.csproj` (Windows)
5. Тести: `cd src && dotnet test FairFare.Tests` (потрібен Docker)

Інший сервер БД: змінна середовища `FAIRFARE_CONNECTION`.
Пароль у docker-compose.yml призначений лише для локальної розробки.

Схема БД: [docs/db-schema.md](docs/db-schema.md)
