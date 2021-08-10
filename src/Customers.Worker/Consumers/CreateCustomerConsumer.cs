using System.Threading.Tasks;
using Customers.Contracts;
using MassTransit;

namespace Customers.Worker.Consumers
{
    public class CreateCustomerConsumer : IConsumer<CreateCustomer>
    {
        public Task Consume(ConsumeContext<CreateCustomer> context)
        {
            return Task.CompletedTask;
        }
    }
}