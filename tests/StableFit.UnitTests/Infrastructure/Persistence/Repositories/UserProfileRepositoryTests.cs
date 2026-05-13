using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StableFit.Domain.Entities;
using StableFit.Infrastructure.Persistence;
using StableFit.Infrastructure.Persistence.Repositories;

namespace StableFit.UnitTests.Infrastructure.Persistence.Repositories;

public class UserProfileRepositoryTests
{
    private StableFitDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<StableFitDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new StableFitDbContext(options);
    }

    [Fact]
    public async Task AddAsync_ThenGetAll_ReturnsAddedProfile()
    {
        await using var db = GetInMemoryDbContext();
        var repo = new UserProfileRepository(db);
        var profile = UserProfile.Create("user-1", "testuser", "Test User", "test@example.com");

        await repo.AddAsync(profile, CancellationToken.None);
        await db.SaveChangesAsync();

        var all = await repo.GetAllAsync(CancellationToken.None);

        all.Should().HaveCount(1);
        all[0].Username.Should().Be("testuser");
    }

    [Fact]
    public async Task GetByIdAsync_ProfileExists_ReturnsProfile()
    {
        await using var db = GetInMemoryDbContext();
        var profile = UserProfile.Create("user-1", "testuser", "Test User", "test@example.com");
        db.UserProfiles.Add(profile);
        await db.SaveChangesAsync();

        var repo = new UserProfileRepository(db);
        var result = await repo.GetByIdAsync(profile.Id, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(profile.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ProfileNotFound_ReturnsNull()
    {
        await using var db = GetInMemoryDbContext();
        var repo = new UserProfileRepository(db);

        var result = await repo.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserIdAsync_ProfileExists_ReturnsProfile()
    {
        await using var db = GetInMemoryDbContext();
        var profile = UserProfile.Create("user-1", "testuser", "Test User", "test@example.com");
        db.UserProfiles.Add(profile);
        await db.SaveChangesAsync();

        var repo = new UserProfileRepository(db);
        var result = await repo.GetByUserIdAsync("user-1", CancellationToken.None);

        result.Should().NotBeNull();
        result!.UserId.Should().Be("user-1");
    }

    [Fact]
    public async Task GetByUserIdAsync_NotFound_ReturnsNull()
    {
        await using var db = GetInMemoryDbContext();
        var repo = new UserProfileRepository(db);

        var result = await repo.GetByUserIdAsync("nonexistent", CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUsernameAsync_ProfileExists_ReturnsProfile()
    {
        await using var db = GetInMemoryDbContext();
        var profile = UserProfile.Create("user-1", "testuser", "Test User", "test@example.com");
        db.UserProfiles.Add(profile);
        await db.SaveChangesAsync();

        var repo = new UserProfileRepository(db);
        var result = await repo.GetByUsernameAsync("  testuser  ", CancellationToken.None); // test trimming

        result.Should().NotBeNull();
        result!.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task GetByEmailAsync_ProfileExists_ReturnsProfile()
    {
        await using var db = GetInMemoryDbContext();
        var profile = UserProfile.Create("user-1", "testuser", "Test User", "test@example.com");
        db.UserProfiles.Add(profile);
        await db.SaveChangesAsync();

        var repo = new UserProfileRepository(db);
        // Test case-insensitive + trimming
        var result = await repo.GetByEmailAsync("  TEST@EXAMPLE.COM  ", CancellationToken.None);

        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetAllAsync_NoProfiles_ReturnsEmptyList()
    {
        await using var db = GetInMemoryDbContext();
        var repo = new UserProfileRepository(db);

        var result = await repo.GetAllAsync(CancellationToken.None);

        result.Should().BeEmpty();
    }
}
