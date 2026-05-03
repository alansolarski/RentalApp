# RentalApp

A peer-to-peer rental marketplace built with .NET MAUI for the SET09102 coursework at Edinburgh Napier University. Community members can list items, search for things nearby, request rentals, and leave reviews.

## Prerequisites

- Docker Desktop
- Android Emulator (Pixel 10, API 37) running on the host machine
- VS Code with the Dev Containers extension
- ADB running on the host

## Setup

Clone the repo and open the folder in VS Code. When prompted, click "Reopen in Container". The first build takes a few minutes.

Once inside the container, copy `RentalApp.Database/appsettings.json.template` to `appsettings.json` and set the connection string to:

```
Host=10.0.2.2:5432;Username=app_user;Password=app_password;Database=appdb
```

If you're running migrations (rather than just building the app), temporarily change the host to `db` instead.

If this is a fresh setup or you're coming from a previous Docker environment, run this in Windows PowerShell on the host first:

```
docker compose down -v
```

## Building and Running

From inside the Dev Container terminal:

```
dotnet build -c Debug
adb install -r /workspace/RentalApp/bin/Debug/net10.0-android/com.companyname.RentalApp-Signed.apk
```

Then launch the app from the emulator. Register an account or log in. The app authenticates against the shared course API at `https://set09102-api.b-davison.workers.dev`.

## Running Tests

```
dotnet test
```

To get a coverage report:

```
dotnet test --collect:"XPlat Code Coverage" --settings RentalApp.Test/coverlet.runsettings
```

## Running Migrations

Migrations need to be run from the `RentalApp.Migrations` directory, with `Host=db` in `appsettings.json`:

```
cd RentalApp.Migrations
dotnet ef database update --project ../RentalApp.Database --startup-project .
```

## Architecture

The app follows MVVM with a service and repository layer. Views bind to ViewModels, ViewModels call services, services call repositories or the API directly.

The main structural decision worth noting is services and non-MAUI interfaces live in `RentalApp.Database` rather than `RentalApp`. The reason is that `RentalApp` targets `net10.0-android` only, which means the test project can't reference it. Moving shared logic to the database library keeps everything testable without platform dependencies.

There are two `IItemRepository` implementations. `ApiItemRepository` is used at runtime and delegates to `ApiService`, while `ItemRepository` uses EF Core and is used in integration tests.

## API Reference

Full endpoint documentation is at `https://set09102-api.b-davison.workers.dev` (Swagger UI at the root).

## Repository

<https://github.com/alansolarski/RentalApp>
