using System.Threading.Tasks;
using Contracts;
using MassTransit;

namespace Company.Consumers
{
    public class CreateCustomerConsumer :
        IConsumer<CreateCustomer>
    {
        public Task Consume(ConsumeContext<CreateCustomer> context)
        {
            return Task.CompletedTask;
        }
    }
}