using Microsoft.EntityFrameworkCore;
using StableFit.Domain;

namespace StableFit.Infrastructure;

public sealed class StableFitDbContext : DbContext
{
    public StableFitDbContext(DbContextOptions<StableFitDbContext> options) : base(options)
    {
    }

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.ToTable("user_profiles");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            entity.Property(x => x.DisplayName)
                .HasColumnName("display_name")
                .HasMaxLength(200)
                .IsRequired();

            entity.HasIndex(x => x.DisplayName)
                .HasDatabaseName("ix_user_profiles_display_name");
        });
    }
}
