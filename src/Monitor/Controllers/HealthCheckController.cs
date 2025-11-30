using Microsoft.AspNetCore.Mvc;

namespace MonitorServer.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthCheckController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Healthy");
        }
    }
}
