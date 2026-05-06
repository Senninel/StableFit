using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StableFit.Infrastructure.Persistence.Entities;

namespace StableFit.Infrastructure.Persistence.Configurations;

public sealed class MatchRecommendationConfiguration : IEntityTypeConfiguration<MatchRecommendation>
{
    public void Configure(EntityTypeBuilder<MatchRecommendation> builder)
    {
        builder.ToTable("match_recommendations");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.RunId)
            .HasColumnName("run_id")
            .IsRequired();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(x => x.RecommendedUserId)
            .HasColumnName("recommended_user_id")
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(x => x.Rank)
            .HasColumnName("rank")
            .IsRequired();

        builder.Property(x => x.Score)
            .HasColumnName("score")
            .IsRequired();

        builder.HasIndex(x => new { x.RunId, x.UserId, x.RecommendedUserId })
            .IsUnique()
            .HasDatabaseName("ux_match_recommendations_run_user_recommended");

        builder.HasIndex(x => new { x.RunId, x.UserId, x.Rank })
            .IsUnique()
            .HasDatabaseName("ux_match_recommendations_run_user_rank");

        builder.HasIndex(x => x.RunId)
            .HasDatabaseName("ix_match_recommendations_run_id");
    }
}
