using Microsoft.EntityFrameworkCore;
using StableFit.Application.Interfaces;
using StableFit.Domain.Entities;

namespace StableFit.Infrastructure;

public sealed class StableFitDbContext : DbContext, IApplicationDbContext
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

            entity.Property(x => x.Username)
                .HasColumnName("username")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Email)
                .HasColumnName("email")
                .HasMaxLength(320)
                .IsRequired();

            entity.HasIndex(x => x.Username)
                .IsUnique()
                .HasDatabaseName("ux_user_profiles_username");

            entity.HasIndex(x => x.Email)
                .IsUnique()
                .HasDatabaseName("ux_user_profiles_email");
        });
    }
}
