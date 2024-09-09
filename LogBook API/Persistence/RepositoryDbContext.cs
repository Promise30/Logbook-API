using LogBook_API.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LogBook_API.Persistence
{
    public sealed class RepositoryDbContext : IdentityDbContext<User>
    {
        
        public RepositoryDbContext(DbContextOptions options) : base(options)
        {
            //Database.EnsureDeleted();
            //Database.EnsureCreated();
        }
        public DbSet<LogbookEntry> LogbookEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(RepositoryDbContext).Assembly);
            modelBuilder.Entity<IdentityRole>().HasData(
             new IdentityRole
             {
                 Name = "User",
                 ConcurrencyStamp = "1",
                 NormalizedName = "USER"
             },
             new IdentityRole
             {
                 Name = "Administrator",
                 ConcurrencyStamp = "2",
                 NormalizedName = "ADMINISTRATOR"
             });
        }
    }
}
