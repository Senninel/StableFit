using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StableFit.Domain.Entities;
using StableFit.Domain.Enums;

namespace StableFit.Infrastructure.Persistence.Configurations;

public sealed class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("user_profiles");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasMaxLength(450)
            .IsRequired();

        builder.HasIndex(x => x.UserId)
            .IsUnique()
            .HasDatabaseName("ux_user_profiles_user_id");

        builder.Property(x => x.Username)
            .HasColumnName("username")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasColumnName("email")
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(x => x.Bio)
            .HasColumnName("bio")
            .HasMaxLength(500);

        builder.Property(x => x.Goal)
            .HasColumnName("goal")
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.ScheduleDays)
            .HasColumnName("schedule_days")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<DayOfWeek>>(v, (JsonSerializerOptions?)null) ?? new List<DayOfWeek>());

        builder.Property(x => x.AgeYears)
            .HasColumnName("age_years");

        builder.Property(x => x.WeightKg)
            .HasColumnName("weight_kg");

        builder.HasIndex(x => x.Username)
            .IsUnique()
            .HasDatabaseName("ux_user_profiles_username");

        builder.HasIndex(x => x.Email)
            .IsUnique()
            .HasDatabaseName("ux_user_profiles_email");
    }
}
