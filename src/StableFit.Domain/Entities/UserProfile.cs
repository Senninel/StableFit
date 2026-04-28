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

    public void Update(string? bio, FitnessGoal? goal, List<DayOfWeek>? scheduleDays, int? ageYears, double? weightKg)
    {
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
    }
}
