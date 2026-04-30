using Microsoft.EntityFrameworkCore;
using RentalApp.Database.Data;

namespace RentalApp.Test.Fixtures;

/// <summary>
/// Provides a fresh in-memory AppDbContext for each test class that uses it.
/// Using a new Guid as the database name means every test class gets its own
/// isolated in-memory store — tests can't bleed state into each other.
/// </summary>
public class DatabaseFixture : IDisposable
{
    public AppDbContext Context { get; private set; }

    public DatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new AppDbContext(options);
        // EnsureCreated applies the model schema to the in-memory store so
        // navigation properties and constraints work the same as with a real DB.
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Dispose();
    }
}
