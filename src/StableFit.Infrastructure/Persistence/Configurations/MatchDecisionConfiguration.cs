using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StableFit.Infrastructure.Persistence.Entities;

namespace StableFit.Infrastructure.Persistence.Configurations;

public sealed class MatchDecisionConfiguration : IEntityTypeConfiguration<MatchDecision>
{
    public void Configure(EntityTypeBuilder<MatchDecision> builder)
    {
        builder.ToTable("match_decisions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.RunId)
            .HasColumnName("run_id")
            .IsRequired();

        builder.Property(x => x.FromUserId)
            .HasColumnName("from_user_id")
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(x => x.ToUserId)
            .HasColumnName("to_user_id")
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(x => x.Decision)
            .HasColumnName("decision")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.HasIndex(x => new { x.RunId, x.FromUserId, x.ToUserId })
            .IsUnique()
            .HasDatabaseName("ux_match_decisions_run_from_to");

        builder.HasIndex(x => new { x.RunId, x.ToUserId })
            .HasDatabaseName("ix_match_decisions_run_to");
    }
}

