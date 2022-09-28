using BlogAPI.Model;
using Microsoft.EntityFrameworkCore;

namespace BlogAPI.Persistence;

public class DataContext:DbContext
{
    public DbSet<BlogPost> BlogPosts { get; set;}
    public DbSet<Author> Authors { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DataContext()
    {
    }

    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseNpgsql("Server=127.0.0.1;Port=5432;Database=Blog;UserId=postgres;");
    }
}