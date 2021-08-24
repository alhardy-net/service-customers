using System;
using System.Threading.Tasks;
using Customers.Contracts;
using MassTransit;

namespace Customers.Api.Extensions
{
    public static class BusExtensions
    {
        public static async Task CreateCustomer(this IBus bus, CreateCustomer command)
        {
            var uri = new Uri("queue:create-customer");
            var endpoint = await bus.GetSendEndpoint(uri);
            await endpoint.Send(command);
        }
    }
}