using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RentalApp.Database.Models;

namespace RentalApp.Database.Data;

/// <summary>
/// EF Core database context for the rental app. Covers Users, Roles, UserRoles, Items,
/// and Rentals. Reviews aren't tracked here — they live entirely through the API.
/// </summary>
/// <remarks>
/// Connection string resolution works in two ways: first it checks the CONNECTION_STRING
/// environment variable (used by the migrations runner and CI), and if that's empty it falls
/// back to the embedded appsettings.json in the assembly. The embedded file approach means
/// we don't need a separate config file when running the migrations tool locally.
/// </remarks>
public class AppDbContext : DbContext
{
    /// <summary>Parameterless constructor required by EF Core tooling (migrations, scaffolding).</summary>
    public AppDbContext()
    { }

    /// <summary>Constructor used at runtime when DI injects configured options.</summary>
    /// <param name="options">EF Core options, typically configured in MauiProgram or tests.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // If DI already configured the context (e.g. tests or MauiProgram), skip this entirely.
        if (optionsBuilder.IsConfigured) return;

        // CI/prod injects the connection string via environment variable.
        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

        if (string.IsNullOrEmpty(connectionString))
        {
            // Fall back to the embedded appsettings.json baked into this assembly.
            // This makes the migrations runner work without any extra config files.
            var a = Assembly.GetExecutingAssembly();
            using var stream = a.GetManifestResourceStream("RentalApp.Database.appsettings.json");

            var config = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();

            connectionString = config.GetConnectionString("DevelopmentConnection");
        }

        optionsBuilder.UseNpgsql(connectionString);
    }

    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<Rental> Rentals { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PasswordSalt).HasMaxLength(255);
        });

        // Configure Role entity
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        // Configure UserRole entity — composite unique index prevents duplicate assignments.
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();

            entity.HasOne(ur => ur.User)
                  .WithMany(u => u.UserRoles)
                  .HasForeignKey(ur => ur.UserId);

            entity.HasOne(ur => ur.Role)
                  .WithMany(r => r.UserRoles)
                  .HasForeignKey(ur => ur.RoleId);
        });

        // Configure Item entity — precision(10,2) on DailyRate avoids floating-point rounding in Postgres.
        modelBuilder.Entity<Item>(entity =>
        {
            entity.Property(e => e.Title).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.DailyRate).HasPrecision(10, 2);
        });
    }
}
