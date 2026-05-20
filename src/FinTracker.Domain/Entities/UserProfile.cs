namespace FinTracker.Domain.Entities;

/// <summary>
/// 1:1 с пользователем Identity (UserId).
/// </summary>
public class UserProfile
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PreferredCurrency { get; set; } = "RUB";
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
