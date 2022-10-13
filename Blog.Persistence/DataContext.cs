using Blog.Models;
using Microsoft.EntityFrameworkCore;

namespace Blog.Persistence;

public class DataContext : DbContext
{
    public DataContext()
    {
    }

    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
    }

    public DbSet<BlogPost> BlogPosts { get; set; }
    public DbSet<Author> Authors { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Role> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new EntityTypeConfigurations.BlogPost());
        modelBuilder.ApplyConfiguration(new EntityTypeConfigurations.Category());
        modelBuilder.ApplyConfiguration(new EntityTypeConfigurations.Author());
        modelBuilder.ApplyConfiguration(new EntityTypeConfigurations.Role());
    }
}