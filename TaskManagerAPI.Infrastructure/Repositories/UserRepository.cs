using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Core.Entities;
using TaskManagerAPI.Core.Interfaces;
using TaskManagerAPI.Infrastructure.Data;

namespace TaskManagerAPI.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context) => _context = context;

    public async Task<ApplicationUser?> GetByEmailAsync(string email) =>
        await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

    public async Task<ApplicationUser?> GetByIdAsync(string id) =>
        await _context.Users.FindAsync(id);

    public async Task<bool> EmailExistsAsync(string email) =>
        await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());

    public async Task<ApplicationUser> CreateAsync(ApplicationUser user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }
}
