using Microsoft.EntityFrameworkCore;
using ShortlinkApi.Models;

namespace ShortlinkApi.Data
{
    public class AppDb : DbContext
    {
        public AppDb(DbContextOptions<AppDb> opts) : base(opts) { }
        public DbSet<Link> Links => Set<Link>();
        public DbSet<Click> Clicks => Set<Click>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.Entity<Link>().HasIndex(l => l.Code).IsUnique();
            mb.Entity<Click>().HasIndex(c => new { c.LinkId, c.Ts });
        }
    }
}
