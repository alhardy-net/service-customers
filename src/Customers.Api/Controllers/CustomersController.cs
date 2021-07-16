using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Customers.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(ILogger<CustomersController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{id}")]
        public Task<Customer> Get(int id)
        {
            return Task.FromResult(new Customer { FirstName = "Allan", Id = 1, LastName = "Hardy" });
        }

        [HttpPost]
        public async Task Post(Customer customer)
        {
            await Task.CompletedTask;
        }

        [HttpDelete]
        public async Task<ActionResult> Delete(int id)
        {
            await Task.CompletedTask;

            return Ok();
        }
    }
}