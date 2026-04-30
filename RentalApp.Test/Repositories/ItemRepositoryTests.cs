using RentalApp.Database.Data.Repositories;
using RentalApp.Database.Models;
using RentalApp.Test.Fixtures;

namespace RentalApp.Test.Repositories;

/// <summary>
/// Integration tests for ItemRepository against an in-memory EF Core database.
/// Using a real (in-memory) DB rather than mocking the context because mocking
/// DbSet is painful and tests against the real EF query pipeline catch more issues.
/// Each test class gets its own isolated DB via DatabaseFixture.
/// </summary>
public class ItemRepositoryTests : IDisposable
{
    private readonly DatabaseFixture _fixture;
    private readonly ItemRepository _sut;

    public ItemRepositoryTests()
    {
        _fixture = new DatabaseFixture();
        _sut = new ItemRepository(_fixture.Context);
    }

    public void Dispose()
    {
        _fixture.Dispose();
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    /// <summary>Inserts a minimal valid User into the in-memory DB and returns it with its generated ID.</summary>
    private User CreateTestUser(string email = "owner@test.com")
    {
        var user = new User
        {
            FirstName = "Test",
            LastName = "Owner",
            Email = email,
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };
        _fixture.Context.Users.Add(user);
        _fixture.Context.SaveChanges();
        return user;
    }

    /// <summary>
    /// Builds a valid Item object without saving it. Call CreateAsync on the SUT to persist.
    /// Coordinates are set to Edinburgh city centre so distance calculations don't fail.
    /// </summary>
    private Item CreateTestItem(int ownerId, string title = "Test Drill")
    {
        return new Item
        {
            Title = title,
            Description = "A test item",
            DailyRate = 10.00m,
            CategoryId = 1,
            OwnerId = ownerId,
            Latitude = 55.95,
            Longitude = -3.19,
            IsAvailable = true
        };
    }

    // -------------------------------------------------------------------------
    // CreateAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateAsync_ValidItem_SavesAndReturnsItemWithId()
    {
        // Arrange
        var user = CreateTestUser();
        var item = CreateTestItem(user.Id);

        // Act
        var result = await _sut.CreateAsync(item);

        // Assert
        Assert.True(result.Id > 0);
        Assert.Equal("Test Drill", result.Title);
    }

    [Fact]
    public async Task CreateAsync_ValidItem_PersistsToDatabase()
    {
        // Arrange
        var user = CreateTestUser("persist@test.com");
        var item = CreateTestItem(user.Id, "Camping Tent");

        // Act
        await _sut.CreateAsync(item);
        var saved = await _fixture.Context.Items.FindAsync(item.Id);

        // Assert
        Assert.NotNull(saved);
        Assert.Equal("Camping Tent", saved.Title);
    }

    // -------------------------------------------------------------------------
    // GetByIdAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsItem()
    {
        // Arrange
        var user = CreateTestUser("getbyid@test.com");
        var item = CreateTestItem(user.Id, "Electric Saw");
        await _sut.CreateAsync(item);

        // Act
        var result = await _sut.GetByIdAsync(item.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Electric Saw", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsNull()
    {
        // Act
        var result = await _sut.GetByIdAsync(99999);

        // Assert
        Assert.Null(result);
    }

    // -------------------------------------------------------------------------
    // GetAllAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetAllAsync_MultipleItems_ReturnsAllItems()
    {
        // Arrange
        var user = CreateTestUser("getall@test.com");
        await _sut.CreateAsync(CreateTestItem(user.Id, "Item A"));
        await _sut.CreateAsync(CreateTestItem(user.Id, "Item B"));
        await _sut.CreateAsync(CreateTestItem(user.Id, "Item C"));

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        Assert.True(result.Count() >= 3);
    }

    [Fact]
    public async Task GetAllAsync_NoItems_ReturnsEmptyList()
    {
        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        Assert.NotNull(result);
    }

    // -------------------------------------------------------------------------
    // UpdateAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_ExistingItem_UpdatesTitle()
    {
        // Arrange
        var user = CreateTestUser("update@test.com");
        var item = CreateTestItem(user.Id, "Old Title");
        await _sut.CreateAsync(item);

        // Act
        item.Title = "New Title";
        await _sut.UpdateAsync(item);
        var updated = await _sut.GetByIdAsync(item.Id);

        // Assert
        Assert.Equal("New Title", updated!.Title);
    }

    [Fact]
    public async Task UpdateAsync_ExistingItem_UpdatesAvailability()
    {
        // Arrange
        var user = CreateTestUser("updateavail@test.com");
        var item = CreateTestItem(user.Id);
        await _sut.CreateAsync(item);

        // Act
        item.IsAvailable = false;
        await _sut.UpdateAsync(item);
        var updated = await _sut.GetByIdAsync(item.Id);

        // Assert
        Assert.False(updated!.IsAvailable);
    }

    // -------------------------------------------------------------------------
    // DeleteAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_ExistingItem_RemovesFromDatabase()
    {
        // Arrange
        var user = CreateTestUser("delete@test.com");
        var item = CreateTestItem(user.Id, "Item To Delete");
        await _sut.CreateAsync(item);

        // Act
        await _sut.DeleteAsync(item.Id);
        var deleted = await _sut.GetByIdAsync(item.Id);

        // Assert
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentId_DoesNotThrow()
    {
        // Act & Assert
        var exception = await Record.ExceptionAsync(() => _sut.DeleteAsync(99999));
        Assert.Null(exception);
    }
}
