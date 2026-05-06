using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StableFit.Infrastructure.Persistence.Entities;

namespace StableFit.Infrastructure.Persistence.Configurations;

public sealed class MatchRunConfiguration : IEntityTypeConfiguration<MatchRun>
{
    public void Configure(EntityTypeBuilder<MatchRun> builder)
    {
        builder.ToTable("match_runs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(x => x.ExpiresAtUtc)
            .HasColumnName("expires_at_utc")
            .IsRequired();

        builder.Property(x => x.EligibleUserCount)
            .HasColumnName("eligible_user_count")
            .IsRequired();

        builder.HasIndex(x => x.ExpiresAtUtc)
            .HasDatabaseName("ix_match_runs_expires_at_utc");
    }
}

