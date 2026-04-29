using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Core.Entities;
using TaskManagerAPI.Core.Enums;

namespace TaskManagerAPI.Infrastructure.Data;

/// <summary>
/// Single DbContext for the application. Fluent API configuration lives here
/// rather than in data-annotation attributes on entities, keeping entities clean.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── ApplicationUser ──────────────────────────────────────────────
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.UserName).IsUnique();

            entity.Property(u => u.Email).HasMaxLength(256).IsRequired();
            entity.Property(u => u.UserName).HasMaxLength(50).IsRequired();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Role).HasMaxLength(20).HasDefaultValue("User");
        });

        // ── TaskItem ─────────────────────────────────────────────────────
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Title).HasMaxLength(200).IsRequired();
            entity.Property(t => t.Description).HasMaxLength(2000);

            // Store enums as strings for readability in the DB
            entity.Property(t => t.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .HasDefaultValue(TaskItemStatus.Pending);

            entity.Property(t => t.Priority)
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .HasDefaultValue(TaskPriority.Medium);

            entity.HasIndex(t => t.UserId);
            entity.HasIndex(t => t.Status);
            entity.HasIndex(t => t.Priority);

            // One user → many tasks; deleting user cascades to tasks
            entity.HasOne(t => t.User)
                  .WithMany(u => u.Tasks)
                  .HasForeignKey(t => t.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
