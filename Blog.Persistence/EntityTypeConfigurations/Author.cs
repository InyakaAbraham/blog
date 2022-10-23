using Blog.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Persistence.EntityTypeConfigurations;

public class Author : IEntityTypeConfiguration<Models.Author>
{
    public void Configure(EntityTypeBuilder<Models.Author> builder)
    {
        builder.HasKey(a => a.AuthorId);
        builder.Property(u => u.Username).IsRequired();
        builder.Property(u => u.EmailAddress).IsRequired();
        builder.HasIndex(u => u.EmailAddress).IsUnique();
        builder.HasMany(a => a.BlogPosts)
            .WithOne(b => b.Author).IsRequired();
      
    }
}