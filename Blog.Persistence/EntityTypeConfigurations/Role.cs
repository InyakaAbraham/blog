using Blog.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Persistence.EntityTypeConfigurations;

public class Role : IEntityTypeConfiguration<Models.Role>
{
    public void Configure(EntityTypeBuilder<Models.Role> builder)
    {
        builder.HasKey(r => r.Id);
        builder.HasMany(x => x.Authors)
            .WithMany(y => y.Roles);
        builder.HasData(new object[]
        {
            new Models.Role { Id = UserRole.Author },
            new Models.Role { Id = UserRole.Moderator },
            new Models.Role { Id = UserRole.Administrator }
        });
    }
}