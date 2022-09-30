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
    public DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseNpgsql("Server=127.0.0.1;Port=5432;Database=BlogV2;UserId=postgres;");
    }
}