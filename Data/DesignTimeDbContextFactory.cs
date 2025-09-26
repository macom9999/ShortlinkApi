using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ShortlinkApi.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDb>
    {
        public AppDb CreateDbContext(string[] args)
        {
            var conn = Environment.GetEnvironmentVariable("PG_CONN")
                      ?? throw new Exception("PG_CONN not set");
            var opts = new DbContextOptionsBuilder<AppDb>()
                .UseNpgsql(conn)
                .Options;
            return new AppDb(opts);
        }
    }
}
