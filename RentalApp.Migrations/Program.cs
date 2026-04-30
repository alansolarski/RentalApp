using Microsoft.EntityFrameworkCore;
using RentalApp.Database.Data;

// This is a standalone console app whose only job is to run EF Core migrations
// against the real Postgres database. It's separate from the MAUI app so we can
// run migrations in CI without needing an Android emulator.
//
// Connection string is read from the CONNECTION_STRING environment variable (see
// AppDbContext.OnConfiguring) — set it before running this in CI or locally.
Console.WriteLine("Running migrations...");
using var context = new AppDbContext();
context.Database.Migrate();
Console.WriteLine("Migrations complete.");
