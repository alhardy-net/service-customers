using System;
using System.Threading.Tasks;
using Customers.Contracts;
using Customers.Persistence;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Customers.Api.Controllers
{
    [ApiController]
    [Route("")]
    public class CustomersController : ControllerBase
    {
        private readonly CustomersContext _context;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(CustomersContext context, IPublishEndpoint publishEndpoint, ILogger<CustomersController> logger)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id)
        {
            _logger.LogInformation("Getting customer by id {Id}", id);
            
            var customer = await _context.Customers.FirstOrDefaultAsync(x => x.Id == id);

            if (customer == null) return NoContent();

            return Ok(customer);
        }

        [HttpPost]
        public async Task<ActionResult> Post(CreateCustomer command)
        {
            await _publishEndpoint.Publish(command);

            return Accepted();
        }

        [HttpDelete]
        public async Task<ActionResult> Delete(int id)
        {
            await _publishEndpoint.Publish<DeleteCustomer>(new { Id = id });

            return Ok();
        }
    }
}