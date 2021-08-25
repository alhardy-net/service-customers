using System.Threading.Tasks;
using Customers.Contracts;
using Customers.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Customers.Worker.Components.Consumers
{
    public class DeleteCustomerConsumer : IConsumer<DeleteCustomer>
    {
        private readonly CustomersContext _context;

        public DeleteCustomerConsumer(CustomersContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<DeleteCustomer> context)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(x => x.Id == context.Message.Id);

            if (customer == null) return;

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
        }
    }
}