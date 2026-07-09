using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StableFit.Domain.Entities;

namespace StableFit.Infrastructure.Persistence.Configurations;

public sealed class NutritionEntryConfiguration : IEntityTypeConfiguration<NutritionEntry>
{
    public void Configure(EntityTypeBuilder<NutritionEntry> builder)
    {
        builder.ToTable("nutrition_entries");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(x => x.Date)
            .HasColumnName("date")
            .IsRequired();

        builder.Property(x => x.MealType)
            .HasColumnName("meal_type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.FoodName)
            .HasColumnName("food_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Calories)
            .HasColumnName("calories")
            .IsRequired();

        builder.Property(x => x.ProteinGrams)
            .HasColumnName("protein_grams")
            .IsRequired();

        builder.Property(x => x.CarbGrams)
            .HasColumnName("carb_grams")
            .IsRequired();

        builder.Property(x => x.FatGrams)
            .HasColumnName("fat_grams")
            .IsRequired();

        builder.Property(x => x.ServingDescription)
            .HasColumnName("serving_description")
            .HasMaxLength(50);

        builder.HasIndex(x => new { x.UserId, x.Date })
            .HasDatabaseName("ix_nutrition_entries_user_date");
    }
}
