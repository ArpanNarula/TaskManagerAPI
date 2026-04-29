using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Core.Common;
using TaskManagerAPI.Core.DTOs.Task;
using TaskManagerAPI.Core.Entities;
using TaskManagerAPI.Core.Interfaces;
using TaskManagerAPI.Infrastructure.Data;

namespace TaskManagerAPI.Infrastructure.Repositories;

/// <summary>
/// Handles all task data access. The query pipeline builds up IQueryable predicates
/// before materializing so we only hit the DB once with a fully-formed SQL query.
/// </summary>
public class TaskRepository : Repository<TaskItem>, ITaskRepository
{
    public TaskRepository(AppDbContext context) : base(context) { }

    public async Task<PagedResult<TaskItem>> GetTasksAsync(
        TaskQueryParameters queryParams, string? userId = null)
    {
        IQueryable<TaskItem> query = _context.Tasks
            .Include(t => t.User)
            .AsNoTracking(); // Read-only — skip change tracking for perf

        // ── Scope to user (regular users only see their own tasks) ──
        if (!string.IsNullOrEmpty(userId))
            query = query.Where(t => t.UserId == userId);

        // ── Filters ──────────────────────────────────────────────────
        if (queryParams.Status.HasValue)
            query = query.Where(t => t.Status == queryParams.Status.Value);

        if (queryParams.Priority.HasValue)
            query = query.Where(t => t.Priority == queryParams.Priority.Value);

        // ── Search ───────────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            var term = queryParams.Search.Trim().ToLower();
            query = query.Where(t =>
                t.Title.ToLower().Contains(term) ||
                (t.Description != null && t.Description.ToLower().Contains(term)));
        }

        // ── Sort ─────────────────────────────────────────────────────
        query = queryParams.SortBy?.ToLower() switch
        {
            "title"     => queryParams.Descending ? query.OrderByDescending(t => t.Title)     : query.OrderBy(t => t.Title),
            "priority"  => queryParams.Descending ? query.OrderByDescending(t => t.Priority)  : query.OrderBy(t => t.Priority),
            "status"    => queryParams.Descending ? query.OrderByDescending(t => t.Status)    : query.OrderBy(t => t.Status),
            "updatedat" => queryParams.Descending ? query.OrderByDescending(t => t.UpdatedAt) : query.OrderBy(t => t.UpdatedAt),
            _           => queryParams.Descending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
        };

        // ── Paginate ──────────────────────────────────────────────────
        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        return new PagedResult<TaskItem>
        {
            Items = items,
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        };
    }

    public async Task<TaskItem?> GetByIdWithUserAsync(int id) =>
        await _context.Tasks
            .Include(t => t.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<bool> ExistsAsync(int id, string userId) =>
        await _context.Tasks.AnyAsync(t => t.Id == id && t.UserId == userId);
}
