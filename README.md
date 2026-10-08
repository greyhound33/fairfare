# FairFare

FairFare is a WPF travel-planning and shared-expense application following the layered architecture described in `docs/task3.md` and `docs/task4.md`.

## Solution layout

- `FairFare.Domain`: domain entities and financial models
- `FairFare.BLL`: business logic, validation, route handling, and reporting services
- `FairFare.DAL`: Entity Framework Core data layer with SQLite support
- `FairFare.UI`: WPF desktop application
- `FairFare.Tests`: xUnit tests for financial calculations

## Run

```bash
dotnet restore
dotnet build FairFare.slnx
dotnet run --project FairFare.UI/FairFare.UI.csproj
```