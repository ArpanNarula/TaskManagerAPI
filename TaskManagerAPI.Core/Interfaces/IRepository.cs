using TaskManagerAPI.Core.Common;
using TaskManagerAPI.Core.DTOs.Task;
using TaskManagerAPI.Core.Entities;

namespace TaskManagerAPI.Core.Interfaces;

/// <summary>
/// Generic repository. Keeps CRUD operations consistent across all entities.
/// Using a generic base prevents duplicating boilerplate in every repo.
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<bool> SaveChangesAsync();
}

/// <summary>
/// Task-specific repository. Extends the generic interface with
/// filtered/paginated queries and user-scoped access checks.
/// </summary>
public interface ITaskRepository : IRepository<TaskItem>
{
    Task<PagedResult<TaskItem>> GetTasksAsync(TaskQueryParameters queryParams, string? userId = null);
    Task<TaskItem?> GetByIdWithUserAsync(int id);
    Task<bool> ExistsAsync(int id, string userId);
}

/// <summary>
/// User repository. Handles user lookup and creation independently of Identity
/// so the Core layer doesn't need to reference Microsoft.AspNetCore.Identity.
/// </summary>
public interface IUserRepository
{
    Task<ApplicationUser?> GetByEmailAsync(string email);
    Task<ApplicationUser?> GetByIdAsync(string id);
    Task<bool> EmailExistsAsync(string email);
    Task<ApplicationUser> CreateAsync(ApplicationUser user);
}
