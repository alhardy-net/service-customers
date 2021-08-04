using System.Threading.Tasks;
using Customers.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Customers.Api.Controllers
{
    [ApiController]
    [Route("")]
    public class CustomersController : ControllerBase
    {
        private readonly CustomersContext _context;
        private readonly IConfiguration _configuration;

        public CustomersController(CustomersContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(x => x.Id == id);

            if (customer == null) return NoContent();

            return Ok(customer);
        }

        [HttpPost]
        public async Task<ActionResult> Post(Customer customer)
        {
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = customer.Id }, customer);
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