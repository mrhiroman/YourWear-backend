using Microsoft.EntityFrameworkCore;
using YourWear_backend.Entities;

namespace YourWear_backend;

public class AppDbContext: DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<PublishedWear> PublishedWears { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<EditableObject> EditableObjects { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
       
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>()
            .HasIndex(p => new { p.Name })
            .IsUnique(true);
    }
}