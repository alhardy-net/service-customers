using System;
using System.Threading.Tasks;
using Customers.Contracts;
using Customers.Persistence;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Customers.Api.Controllers
{
    [ApiController]
    [Route("")]
    public class CustomersController : ControllerBase
    {
        private readonly CustomersContext _context;
        private readonly IBus _bus;

        public CustomersController(CustomersContext context, IBus bus)
        {
            _context = context;
            _bus = bus;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(x => x.Id == id);

            if (customer == null) return NoContent();

            return Ok(customer);
        }

        [HttpPost]
        public async Task<ActionResult> Post(CreateCustomer command)
        {
            var uri = new Uri("queue:create-customer");
            var endpoint = await _bus.GetSendEndpoint(uri);
            await endpoint.Send(command);

            return StatusCode(202);
        }

        [HttpDelete]
        public async Task<ActionResult> Delete(int id)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(x => x.Id == id);

            if (customer == null) return Ok();

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}