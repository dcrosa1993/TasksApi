using Microsoft.EntityFrameworkCore;
using TasksApi.Api.Domain.Entities;

namespace TasksApi.Api.Infrastructure.Persistence;

public sealed class AppDbContext(
    DbContextOptions<AppDbContext> options
) : DbContext(options)
{
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var task = modelBuilder.Entity<TaskItem>();

        task.HasKey(x => x.Id);

        task.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(120);

        task.Property(x => x.Description)
            .HasMaxLength(1000);

        task.Property(x => x.Status)
            .IsRequired();

        task.Property(x => x.Responsible)
            .IsRequired()
            .HasMaxLength(100);

        task.Property(x => x.DueDate)
            .IsRequired();

        task.HasIndex(x => x.Status);

        task.HasIndex(x => x.Responsible);
    }
}