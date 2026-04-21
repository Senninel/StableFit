namespace StableFit.Domain;

public sealed class UserProfile
{
    public Guid Id { get; private set; }

    public string DisplayName { get; private set; }

    private UserProfile(Guid id, string displayName)
    {
        Id = id;
        DisplayName = displayName;
    }

    public static UserProfile Create(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Display name is required.", nameof(displayName));
        }

        return new UserProfile(Guid.NewGuid(), displayName.Trim());
    }

    // For ORMs/serializers. Kept private to preserve invariants.
    private UserProfile()
    {
        DisplayName = string.Empty;
    }
}
