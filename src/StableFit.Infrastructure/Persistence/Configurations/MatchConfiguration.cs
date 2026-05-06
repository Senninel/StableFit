using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StableFit.Infrastructure.Persistence.Entities;

namespace StableFit.Infrastructure.Persistence.Configurations;

public sealed class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("matches");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.UserId1)
            .HasColumnName("user_id_1")
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(x => x.UserId2)
            .HasColumnName("user_id_2")
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(x => new { x.UserId1, x.UserId2 })
            .IsUnique()
            .HasDatabaseName("ux_matches_user1_user2");

        // Helpful for filtering active matches quickly.
        builder.HasIndex(x => x.Status)
            .HasDatabaseName("ix_matches_status");

        builder.HasIndex(x => x.UserId1)
            .HasDatabaseName("ix_matches_user_id_1");

        builder.HasIndex(x => x.UserId2)
            .HasDatabaseName("ix_matches_user_id_2");
    }
}
