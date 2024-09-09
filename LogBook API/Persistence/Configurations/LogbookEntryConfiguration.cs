using LogBook_API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace LogBook_API.Persistence.Configurations
{
    internal sealed class LogbookEntryConfiguration : IEntityTypeConfiguration<LogbookEntry>
    {
        public void Configure(EntityTypeBuilder<LogbookEntry> builder) 
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Activity)
                    .HasMaxLength(50)
                    .IsRequired();
            builder.Property(e => e.Description)
                   .HasMaxLength(200);
            builder.Property(e => e.CreatedDate)
                   .HasDefaultValueSql("GETDATE()");
            builder.Property(e => e.LastUpdatedDate)
                   .HasDefaultValueSql("GETDATE()");
            builder.HasOne(e => e.User)
                   .WithMany(e => e.LogbookEntries)
                   .HasForeignKey(e => e.UserId);

        }
    }
}
