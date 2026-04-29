using System.ComponentModel.DataAnnotations;
using TaskManagerAPI.Core.Enums;

namespace TaskManagerAPI.Core.DTOs.Task;

public class CreateTaskDto
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
}

public class UpdateTaskDto
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    public TaskItemStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
}

public class TaskResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
}

/// <summary>
/// Query parameters for the GET /tasks endpoint.
/// All filters are optional — omitting them returns all tasks.
/// </summary>
public class TaskQueryParameters
{
    // Filtering
    public TaskItemStatus? Status { get; set; }
    public TaskPriority? Priority { get; set; }

    // Search (case-insensitive, matches Title or Description)
    public string? Search { get; set; }

    // Pagination
    private int _page = 1;
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    private int _pageSize = 10;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > 50 ? 50 : value < 1 ? 1 : value;
    }

    // Sorting
    public string SortBy { get; set; } = "CreatedAt";
    public bool Descending { get; set; } = true;
}
