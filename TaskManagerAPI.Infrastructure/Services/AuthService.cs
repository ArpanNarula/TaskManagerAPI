using Microsoft.Extensions.Logging;
using TaskManagerAPI.Core.DTOs.Auth;
using TaskManagerAPI.Core.Entities;
using TaskManagerAPI.Core.Interfaces;

namespace TaskManagerAPI.Infrastructure.Services;

/// <summary>
/// Handles user registration and login.
/// Passwords are hashed with BCrypt (work factor 12) — never stored in plaintext.
/// Throws InvalidOperationException for domain-level errors; the global middleware
/// catches these and maps them to 400/401 responses.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepo,
        IJwtService jwtService,
        ILogger<AuthService> logger)
    {
        _userRepo = userRepo;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto)
    {
        if (await _userRepo.EmailExistsAsync(dto.Email))
            throw new InvalidOperationException("Email is already registered.");

        var user = new ApplicationUser
        {
            UserName     = dto.UserName,
            Email        = dto.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 12),
            Role         = "User"
        };

        await _userRepo.CreateAsync(user);
        _logger.LogInformation("New user registered: {Email}", user.Email);

        return BuildAuthResponse(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
    {
        var user = await _userRepo.GetByEmailAsync(dto.Email);

        // Deliberate: same error message for "not found" and "wrong password"
        // to prevent user-enumeration attacks.
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        _logger.LogInformation("User logged in: {Email}", user.Email);
        return BuildAuthResponse(user);
    }

    private AuthResponseDto BuildAuthResponse(ApplicationUser user) => new()
    {
        Token     = _jwtService.GenerateToken(user),
        UserName  = user.UserName,
        Email     = user.Email,
        Role      = user.Role,
        ExpiresAt = _jwtService.GetExpiry()
    };
}
