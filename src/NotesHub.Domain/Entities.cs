namespace NotesHub.Domain;

public enum UserTier { Free, Premium, Enterprise }

public abstract class Entity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; private set; }
    protected void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}

public sealed class Note : Entity
{
    public required string OwnerId { get; init; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public bool IsFavorite { get; private set; }
    public void Update(string title, string content, bool isFavorite)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.", nameof(title));
        Title = title.Trim(); Content = content; IsFavorite = isFavorite; Touch();
    }
}

public sealed class StickyNote : Entity
{
    public required string OwnerId { get; init; }
    public string Content { get; private set; } = string.Empty;
    public string Color { get; private set; } = "#fff59d";
    public double PositionX { get; private set; }
    public double PositionY { get; private set; }
    public void Update(string content, string color, double positionX, double positionY)
    { Content = content; Color = color; PositionX = positionX; PositionY = positionY; Touch(); }
}

public sealed class RefreshToken : Entity
{
    public required string UserId { get; init; }
    public required string TokenHash { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public bool IsActive => RevokedAt is null && ExpiresAt > DateTimeOffset.UtcNow;
    public void Revoke() => RevokedAt = DateTimeOffset.UtcNow;
}
