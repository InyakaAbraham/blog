using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Persistence.EntityTypeConfigurations;

public class Author : IEntityTypeConfiguration<Models.Author>
{
    public void Configure(EntityTypeBuilder<Models.Author> builder)
    {
        builder.HasKey(x => x.AuthorId);
        builder.Property(x => x.Username).IsRequired();
        builder.Property(x => x.EmailAddress).IsRequired();
        builder.HasIndex(x => x.EmailAddress).IsUnique();
        builder.Property(x => x.VerifiedAt).IsRequired(false);
        builder.HasMany(x => x.BlogPosts)
            .WithOne(y => y.Author).IsRequired();
    }
}