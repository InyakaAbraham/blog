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
        options.UseNpgsql("Server=127.0.0.1;Port=5432;Database=BlogV3;UserId=postgres;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var blogPost = modelBuilder.Entity<BlogPost>();
        var author = modelBuilder.Entity<Author>();
        var category = modelBuilder.Entity<Category>();
        var user = modelBuilder.Entity<User>();

        category.HasKey(c => c.CategoryName);
        category.HasMany(c => c.BlogPosts)
            .WithOne(b => b.Category)
            .HasForeignKey("CategoryName").IsRequired();

        author.HasKey(a => a.AuthorId);
        author.Property(a => a.Name).IsRequired();
        author.HasMany(a => a.BlogPosts)
            .WithOne(b => b.Author).IsRequired();

        user.HasKey(u => u.Username);

        blogPost.HasKey(b => b.PostId);
        blogPost.Property(b => b.Body).IsRequired();
        blogPost.Property(b => b.Title).IsRequired();
        blogPost.Property(b => b.Tags).IsRequired();
        blogPost.HasOne(b => b.Author)
            .WithMany(a => a.BlogPosts);
        blogPost.HasOne(b => b.Category)
            .WithMany(c => c.BlogPosts);
    }
}