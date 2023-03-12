using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Persistence.EntityTypeConfigurations;

public class RecentActivities : IEntityTypeConfiguration<Models.RecentActivities>
{
    public void Configure(EntityTypeBuilder<Models.RecentActivities> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Message).IsRequired();
        builder.Property(x => x.Date).IsRequired();
        builder.Property(x => x.AuthorId).IsRequired();
    }
}
