using JiraLite.Models;
using Microsoft.EntityFrameworkCore;

namespace JiraLite.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    public DbSet<User> Users { get; set; }
    public DbSet<Issue> Issues { get; set; }
    public DbSet<IssueComment> IssueComments { get; set; }
    public DbSet<IssueHistory> IssueHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>()
            .HasMany(u => u.Issues)
            .WithOne()
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Issue>()
            .HasMany(i => i.Comments)
            .WithOne()
            .HasForeignKey(c => c.IssueId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Issue>()
            .HasMany(i => i.History)
            .WithOne()
            .HasForeignKey(h => h.IssueId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }
}