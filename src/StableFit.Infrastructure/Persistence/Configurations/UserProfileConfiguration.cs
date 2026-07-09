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

        var scheduleDaysComparer = new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<DayOfWeek>>(
            (a, b) => a != null && b != null && a.SequenceEqual(b),
            v => v.Aggregate(0, (hash, day) => HashCode.Combine(hash, day.GetHashCode())),
            v => v.ToList());

        builder.Property(x => x.ScheduleDays)
            .HasColumnName("schedule_days")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<DayOfWeek>>(v, (JsonSerializerOptions?)null) ?? new List<DayOfWeek>(),
                scheduleDaysComparer);

        builder.Property(x => x.AgeYears)
            .HasColumnName("age_years");

        builder.Property(x => x.WeightKg)
            .HasColumnName("weight_kg");

        builder.Property(x => x.HeightCm)
            .HasColumnName("height_cm");

        builder.Property(x => x.BiologicalSex)
            .HasColumnName("biological_sex")
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.DailyCalorieTarget)
            .HasColumnName("daily_calorie_target");

        builder.Property(x => x.PreferredGymName)
            .HasColumnName("preferred_gym_name")
            .HasMaxLength(200);

        builder.Property(x => x.PreferredGymAddress)
            .HasColumnName("preferred_gym_address")
            .HasMaxLength(500);

        builder.Property(x => x.PreferredGymLatitude)
            .HasColumnName("preferred_gym_latitude");

        builder.Property(x => x.PreferredGymLongitude)
            .HasColumnName("preferred_gym_longitude");

        builder.HasIndex(x => x.Username)
            .IsUnique()
            .HasDatabaseName("ux_user_profiles_username");

        builder.HasIndex(x => x.Email)
            .IsUnique()
            .HasDatabaseName("ux_user_profiles_email");
    }
}
