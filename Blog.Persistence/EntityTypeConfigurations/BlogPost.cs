using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Persistence.EntityTypeConfigurations;

public class BlogPost : IEntityTypeConfiguration<Models.BlogPost>
{
    public void Configure(EntityTypeBuilder<Models.BlogPost> builder)
    {
        builder.HasKey(b => b.PostId);
        builder.Property(b => b.Body).IsRequired();
        builder.Property(b => b.Title).IsRequired();
        builder.Property(b => b.Tags).IsRequired();
        builder.HasOne(b => b.Author)
            .WithMany(a => a.BlogPosts);
        builder.HasOne(b => b.Category)
            .WithMany(c => c.BlogPosts);
    }
}