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

        [HttpGet("secret/one")]
        public ActionResult<string> ShowSecretOne()
        {
            return Ok(_configuration.GetValue<string>("customers/shared:secretone"));
        }
        
        [HttpGet("secret/two")]
        public ActionResult<string> ShowSecretTwo()
        {
            return Ok(_configuration.GetValue<string>("customers/shared:secrettwo"));
        }
    }
}