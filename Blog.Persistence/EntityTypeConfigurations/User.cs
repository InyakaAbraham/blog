using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Persistence.EntityTypeConfigurations;

public class User:IEntityTypeConfiguration<Models.User>
{
    public void Configure(EntityTypeBuilder<Models.User> builder)
    {
        builder.HasKey(u => u.UserId);
        builder.Property(u => u.Username).IsRequired();
        builder.HasIndex(u => u.Username).IsUnique();
        builder.Property(u => u.EmailAddress).IsRequired();
        builder.HasIndex(u => u.EmailAddress).IsUnique();

    }
}