# FairFare

FairFare is a WPF travel-planning and shared-expense application following the layered architecture described in `docs/task3.md` and `docs/task4.md`.

## Solution layout

- `src/FairFare.Domain`: domain entities and financial models
- `src/FairFare.BLL`: business logic, validation, route handling, and reporting services
- `src/FairFare.DAL`: Entity Framework Core data layer with SQLite support
- `src/FairFare.UI`: WPF desktop application
- `src/FairFare.Tests`: xUnit tests for financial calculations

## Run

```bash
cd src
dotnet restore
dotnet build FairFare.slnx
dotnet run --project FairFare.UI/FairFare.UI.csproj
```