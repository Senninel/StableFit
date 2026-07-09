using StableFit.Domain.Enums;

namespace StableFit.Domain.Entities;

public sealed class UserProfile
{
    public Guid Id { get; private set; }

    /// <summary>FK to the ASP.NET Core Identity user. Set once at creation.</summary>
    public string UserId { get; private set; }

    public string Username { get; private set; }

    public string Name { get; private set; }

    public string Email { get; private set; }

    public string? Bio { get; private set; }

    public FitnessGoal Goal { get; private set; }

    public List<DayOfWeek> ScheduleDays { get; private set; }

    public int? AgeYears { get; private set; }

    public double? WeightKg { get; private set; }

    public int? HeightCm { get; private set; }

    public BiologicalSex? BiologicalSex { get; private set; }

    public int? DailyCalorieTarget { get; private set; }

    public string? PreferredGymName { get; private set; }

    public string? PreferredGymAddress { get; private set; }

    public double? PreferredGymLatitude { get; private set; }

    public double? PreferredGymLongitude { get; private set; }

    private UserProfile(Guid id, string userId, string username, string name, string email)
    {
        Id = id;
        UserId = userId;
        Username = username;
        Name = name;
        Email = email;
        ScheduleDays = [];
    }

    // For ORMs/serializers. Kept private to preserve invariants.
    private UserProfile()
    {
        UserId = string.Empty;
        Username = string.Empty;
        Name = string.Empty;
        Email = string.Empty;
        ScheduleDays = [];
    }

    public static UserProfile Create(string userId, string username, string name, string email)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId is required.", nameof(userId));

        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required.", nameof(username));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));

        return new UserProfile(
            Guid.NewGuid(),
            userId.Trim(),
            username.Trim(),
            name.Trim(),
            email.Trim().ToLowerInvariant());
    }

    public void Update(
        string? bio,
        FitnessGoal? goal,
        List<DayOfWeek>? scheduleDays,
        int? ageYears,
        double? weightKg,
        int? heightCm,
        BiologicalSex? biologicalSex,
        int? dailyCalorieTarget)
    {
        if (ageYears.HasValue && ageYears.Value < 0)
            throw new ArgumentException("Age cannot be negative.", nameof(ageYears));

        if (weightKg.HasValue && weightKg.Value <= 0)
            throw new ArgumentException("Weight must be greater than zero.", nameof(weightKg));

        if (heightCm.HasValue && (heightCm.Value < 50 || heightCm.Value > 300))
            throw new ArgumentException("Height must be between 50 and 300 cm.", nameof(heightCm));

        if (dailyCalorieTarget.HasValue && dailyCalorieTarget.Value <= 0)
            throw new ArgumentException("Daily calorie target must be greater than zero.", nameof(dailyCalorieTarget));

        if (bio is not null)
            Bio = bio;

        if (goal is not null)
            Goal = goal.Value;

        if (scheduleDays is not null)
            ScheduleDays = scheduleDays;

        if (ageYears is not null)
            AgeYears = ageYears;

        if (weightKg is not null)
            WeightKg = weightKg;

        if (heightCm is not null)
            HeightCm = heightCm;

        if (biologicalSex is not null)
            BiologicalSex = biologicalSex;

        if (dailyCalorieTarget is not null)
            DailyCalorieTarget = dailyCalorieTarget;
    }

    public void SetPreferredGym(string name, string? address, double latitude, double longitude)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Gym name is required.", nameof(name));

        PreferredGymName = name.Trim();
        PreferredGymAddress = address?.Trim();
        PreferredGymLatitude = latitude;
        PreferredGymLongitude = longitude;
    }

    public void ClearPreferredGym()
    {
        PreferredGymName = null;
        PreferredGymAddress = null;
        PreferredGymLatitude = null;
        PreferredGymLongitude = null;
    }
}
