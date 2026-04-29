using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagerAPI.Core.Common;
using TaskManagerAPI.Core.DTOs.Task;
using TaskManagerAPI.Core.Interfaces;

namespace TaskManagerAPI.API.Controllers;

/// <summary>
/// Full CRUD for tasks. All endpoints require authentication.
///
/// Authorization model:
///   - Authenticated users create/read/update/delete their own tasks.
///   - Admins can read/update/delete any task and see all tasks in GET /tasks.
///
/// The controller extracts userId and role from JWT claims and passes them
/// to the service — it never applies authorization logic itself.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService) => _taskService = taskService;

    // ── Helpers ───────────────────────────────────────────────────────────

    private string UserId =>
        User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    private string UserRole =>
        User.FindFirstValue(ClaimTypes.Role) ?? "User";

    // ── GET /api/tasks ────────────────────────────────────────────────────

    /// <summary>
    /// Retrieve a paginated, filtered, searchable list of tasks.
    /// Admins see all users' tasks; regular users see only their own.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<TaskResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTasks([FromQuery] TaskQueryParameters queryParams)
    {
        var result = await _taskService.GetTasksAsync(queryParams, UserId, UserRole);
        return Ok(ApiResponse<PagedResult<TaskResponseDto>>.Ok(result));
    }

    // ── GET /api/tasks/{id} ───────────────────────────────────────────────

    /// <summary>Get a single task by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<TaskResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTask([FromRoute] int id)
    {
        var task = await _taskService.GetByIdAsync(id, UserId, UserRole);
        if (task is null)
            return NotFound(ApiResponse<object>.Fail($"Task {id} not found."));

        return Ok(ApiResponse<TaskResponseDto>.Ok(task));
    }

    // ── POST /api/tasks ───────────────────────────────────────────────────

    /// <summary>Create a new task. The authenticated user becomes the owner.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TaskResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail(
                "Validation failed",
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))));

        var created = await _taskService.CreateAsync(dto, UserId);
        return CreatedAtAction(
            nameof(GetTask),
            new { id = created.Id },
            ApiResponse<TaskResponseDto>.Ok(created, "Task created successfully."));
    }

    // ── PUT /api/tasks/{id} ───────────────────────────────────────────────

    /// <summary>Update an existing task (full replacement, not partial).</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<TaskResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateTask([FromRoute] int id, [FromBody] UpdateTaskDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail(
                "Validation failed",
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))));

        var updated = await _taskService.UpdateAsync(id, dto, UserId, UserRole);
        if (updated is null)
            return NotFound(ApiResponse<object>.Fail($"Task {id} not found or access denied."));

        return Ok(ApiResponse<TaskResponseDto>.Ok(updated, "Task updated successfully."));
    }

    // ── DELETE /api/tasks/{id} ────────────────────────────────────────────

    /// <summary>
    /// Delete a task. Admins can delete any task; users only their own.
    /// Returns 204 No Content on success.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask([FromRoute] int id)
    {
        var deleted = await _taskService.DeleteAsync(id, UserId, UserRole);
        if (!deleted)
            return NotFound(ApiResponse<object>.Fail($"Task {id} not found or access denied."));

        return NoContent();
    }
}
