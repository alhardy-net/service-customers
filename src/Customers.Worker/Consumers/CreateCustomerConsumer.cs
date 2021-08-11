using System.Threading.Tasks;
using Customers.Contracts;
using Customers.Persistence;
using MassTransit;

namespace Customers.Worker.Consumers
{
    public class CreateCustomerConsumer : IConsumer<CreateCustomer>
    {
        private readonly CustomersContext _context;

        public CreateCustomerConsumer(CustomersContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<CreateCustomer> context)
        {
            _context.Add(new Customer { FirstName = context.Message.FirstName, LastName = context.Message.LastName });
            await _context.SaveChangesAsync();
        }
    }
}