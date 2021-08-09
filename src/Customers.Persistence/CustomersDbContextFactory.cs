using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Customers.Persistence
{
    public class CustomersDbContextFactory : IDesignTimeDbContextFactory<CustomersContext>
    {
        public CustomersContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true)
                .Build();

            var builder = new DbContextOptionsBuilder<CustomersContext>();

            var connectionString = configuration.GetConnectionString("CustomersContext");
            
            builder.UseNpgsql(connectionString, x => x.MigrationsAssembly(typeof(CustomersDbContextFactory).Assembly.FullName));

            return new CustomersContext(builder.Options);
        }
    }
}