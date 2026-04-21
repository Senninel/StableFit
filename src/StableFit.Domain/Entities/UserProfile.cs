namespace StableFit.Domain.Entities;

public sealed class UserProfile
{
    public Guid Id { get; private set; }

    public string Username { get; private set; }

    public string Name { get; private set; }

    public string Email { get; private set; }

    private UserProfile(Guid id, string username, string name, string email)
    {
        Id = id;
        Username = username;
        Name = name;
        Email = email;
    }

    // For ORMs/serializers. Kept private to preserve invariants.
    private UserProfile()
    {
        Username = string.Empty;
        Name = string.Empty;
        Email = string.Empty;
    }

    public static UserProfile Create(string username, string name, string email)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username is required.", nameof(username));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        var normalizedUsername = username.Trim();
        var normalizedName = name.Trim();
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return new UserProfile(Guid.NewGuid(), normalizedUsername, normalizedName, normalizedEmail);
    }
}
