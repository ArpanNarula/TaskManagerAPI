using Microsoft.Extensions.Logging;
using TaskManagerAPI.Core.Common;
using TaskManagerAPI.Core.DTOs.Task;
using TaskManagerAPI.Core.Entities;
using TaskManagerAPI.Core.Enums;
using TaskManagerAPI.Core.Interfaces;

namespace TaskManagerAPI.Infrastructure.Services;

/// <summary>
/// All task business logic lives here. Controllers are kept thin — they only
/// handle HTTP concerns (routing, model state, claims extraction) and delegate
/// everything else to this service.
///
/// Authorization rules enforced here:
///   - Regular users: can only read/modify their own tasks.
///   - Admins: full access across all users.
/// </summary>
public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepo;
    private readonly ILogger<TaskService> _logger;

    public TaskService(ITaskRepository taskRepo, ILogger<TaskService> logger)
    {
        _taskRepo = taskRepo;
        _logger = logger;
    }

    public async Task<PagedResult<TaskResponseDto>> GetTasksAsync(
        TaskQueryParameters queryParams, string userId, string role)
    {
        // Admins see everything; regular users see only their own tasks
        var scopedUserId = role == "Admin" ? null : userId;

        var pagedTasks = await _taskRepo.GetTasksAsync(queryParams, scopedUserId);

        return new PagedResult<TaskResponseDto>
        {
            Items      = pagedTasks.Items.Select(MapToDto),
            TotalCount = pagedTasks.TotalCount,
            Page       = pagedTasks.Page,
            PageSize   = pagedTasks.PageSize
        };
    }

    public async Task<TaskResponseDto?> GetByIdAsync(int id, string userId, string role)
    {
        var task = await _taskRepo.GetByIdWithUserAsync(id);
        if (task is null) return null;

        // Non-admin requesting someone else's task → treat as not found
        if (role != "Admin" && task.UserId != userId)
            return null;

        return MapToDto(task);
    }

    public async Task<TaskResponseDto> CreateAsync(CreateTaskDto dto, string userId)
    {
        var task = new TaskItem
        {
            Title       = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            Status      = dto.Status,
            Priority    = dto.Priority,
            UserId      = userId,
            CreatedAt   = DateTime.UtcNow,
            UpdatedAt   = DateTime.UtcNow
        };

        var created = await _taskRepo.AddAsync(task);
        _logger.LogInformation("Task {Id} created by user {UserId}", created.Id, userId);

        // Re-fetch to include User navigation property for the response DTO
        var full = await _taskRepo.GetByIdWithUserAsync(created.Id);
        return MapToDto(full!);
    }

    public async Task<TaskResponseDto?> UpdateAsync(
        int id, UpdateTaskDto dto, string userId, string role)
    {
        var task = await _taskRepo.GetByIdWithUserAsync(id);
        if (task is null) return null;

        if (role != "Admin" && task.UserId != userId)
            return null;

        // Re-fetch tracked entity (GetByIdWithUserAsync uses AsNoTracking)
        var trackedTask = await _taskRepo.GetByIdAsync(id);
        if (trackedTask is null) return null;

        trackedTask.Title       = dto.Title.Trim();
        trackedTask.Description = dto.Description?.Trim();
        trackedTask.Status      = dto.Status;
        trackedTask.Priority    = dto.Priority;
        trackedTask.UpdatedAt   = DateTime.UtcNow;

        await _taskRepo.UpdateAsync(trackedTask);
        _logger.LogInformation("Task {Id} updated by user {UserId}", id, userId);

        var updated = await _taskRepo.GetByIdWithUserAsync(id);
        return MapToDto(updated!);
    }

    public async Task<bool> DeleteAsync(int id, string userId, string role)
    {
        var task = await _taskRepo.GetByIdAsync(id);
        if (task is null) return false;

        if (role != "Admin" && task.UserId != userId)
            return false;

        await _taskRepo.DeleteAsync(task);
        _logger.LogInformation("Task {Id} deleted by user {UserId}", id, userId);
        return true;
    }

    // ── Mapping ───────────────────────────────────────────────────────────
    private static TaskResponseDto MapToDto(TaskItem task) => new()
    {
        Id          = task.Id,
        Title       = task.Title,
        Description = task.Description,
        Status      = task.Status.ToString(),
        Priority    = task.Priority.ToString(),
        CreatedAt   = task.CreatedAt,
        UpdatedAt   = task.UpdatedAt,
        UserId      = task.UserId,
        UserName    = task.User?.UserName ?? string.Empty
    };
}
