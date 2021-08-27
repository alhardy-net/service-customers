using System.Threading.Tasks;
using System.Transactions;
using Customers.Contracts;
using Customers.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Customers.Worker.Components.Consumers
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
            var customer = new Customer { FirstName = context.Message.FirstName, LastName = context.Message.LastName };
            _context.Add(customer);
            
            await _context.SaveChangesAsync();

            await context.Publish<CustomerCreated>(new { Id = customer.Id });
        }
    }
}