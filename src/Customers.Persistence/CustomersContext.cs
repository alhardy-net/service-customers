using Microsoft.EntityFrameworkCore;

namespace Customers.Persistence
{
    public class CustomersContext : DbContext
    {
        public CustomersContext(DbContextOptions<CustomersContext> options) : base(options)
        {
        }
        
        public DbSet<Customer> Customers { get; set; }
    }
}