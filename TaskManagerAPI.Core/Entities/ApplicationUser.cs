namespace TaskManagerAPI.Core.Entities;

/// <summary>
/// Represents an application user. Extends ASP.NET Identity via the Infrastructure layer.
/// Kept in Core as a plain entity so the domain stays framework-agnostic.
/// </summary>
public class ApplicationUser
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
