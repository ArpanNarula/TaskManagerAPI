using TaskManagerAPI.Core.Common;
using TaskManagerAPI.Core.DTOs.Auth;
using TaskManagerAPI.Core.DTOs.Task;
using TaskManagerAPI.Core.Entities;

namespace TaskManagerAPI.Core.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto dto);
}

public interface ITaskService
{
    Task<PagedResult<TaskResponseDto>> GetTasksAsync(TaskQueryParameters queryParams, string userId, string role);
    Task<TaskResponseDto?> GetByIdAsync(int id, string userId, string role);
    Task<TaskResponseDto> CreateAsync(CreateTaskDto dto, string userId);
    Task<TaskResponseDto?> UpdateAsync(int id, UpdateTaskDto dto, string userId, string role);
    Task<bool> DeleteAsync(int id, string userId, string role);
}

public interface IJwtService
{
    string GenerateToken(ApplicationUser user);
    DateTime GetExpiry();
}
