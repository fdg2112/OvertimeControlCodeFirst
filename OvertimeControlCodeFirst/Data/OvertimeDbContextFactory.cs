using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace OvertimeControlCodeFirst.Data
{
    public class OvertimeDbContextFactory : IDesignTimeDbContextFactory<OvertimeDbContext>
    {
        public OvertimeDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // importante!
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<OvertimeDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new OvertimeDbContext(optionsBuilder.Options);
        }
    }

}
