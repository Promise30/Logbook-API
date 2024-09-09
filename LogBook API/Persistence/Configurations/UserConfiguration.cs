using LogBook_API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogBook_API.Persistence.Configurations
{
    internal sealed class UserConfiguration : IEntityTypeConfiguration<User> 
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(u => u.FirstName)
                    .HasMaxLength(50)
                    .IsRequired();
            builder.Property(u => u.LastName)
                   .HasMaxLength(50)
                   .IsRequired();
            builder.HasMany(u => u.LogbookEntries)
                   .WithOne(l => l.User)
                   .HasForeignKey(l => l.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.Property(u => u.CreatedDate)
                  .HasDefaultValueSql("GETDATE()");
            builder.Property(u => u.LastUpdatedDate)
                   .HasDefaultValueSql("GETDATE()");

        }
    }
}
