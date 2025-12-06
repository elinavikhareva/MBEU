using Microsoft.EntityFrameworkCore;
using TaskTracker.Models;

namespace TaskTracker.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // Наша таблица задач
    public DbSet<TaskItem> Tasks => Set<TaskItem>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TaskItem>(e =>
        {
            e.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(200);

            e.Property(t => t.Description)
                .HasMaxLength(2000);

            e.Property(t => t.CreatedUtc)
                .IsRequired();

            // Можно добавить индекс по IsDone, CreatedUtc — пригодится потом
            e.HasIndex(t => t.IsDone);
            e.HasIndex(t => t.CreatedUtc);
        });
    }
}