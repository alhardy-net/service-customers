using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Customers.Api.Controllers
{
    [Microsoft.AspNetCore.Components.Route("")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [ApiController]
    public class MetaController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public MetaController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("ping")]
        public ActionResult<string> Ping()
        {
            return Ok("pong");
        }

        [HttpGet("secret")]
        public ActionResult<string> ShowSecret()
        {
            return Ok(_configuration.GetValue<string>("customers/customer-api/supersecret:supersecret"));
        }
    }
}