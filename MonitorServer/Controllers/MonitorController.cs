using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Monitor.Actors;
using Monitor.Messages;
using static MonitorServer.Startup;

namespace MonitorServer.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class MonitorController : ControllerBase
    {
        private readonly ILogger<MonitorController> _logger;
        private readonly IActorRef _monitorManager;

        public MonitorController(MonitorManagerActorProvider monitorManager, ILogger<MonitorController> logger)
        {
            _monitorManager = monitorManager();
            _logger = logger;
        }

        [HttpGet]
        public async Task<IEnumerable<MonitorInfo>> Get()
        {
            _logger.LogDebug("Get monitors");
            var res = await _monitorManager.Ask<ListMonitorMessageRes>(new ListMonitorMessageReq());
            return res.Monitors;
        }

        [HttpGet("{name}")]
        public async Task<ActionResult<MonitorStatusMessageRes>> GetMonitorInfo(string name)
        {
            _logger.LogDebug($"Get monitor status for: {name}");
            var res = await _monitorManager.Ask<MonitorStatusMessageRes>(new MonitorStatusMessageReq(name));
            if(res == null)
            {
                return NotFound($"Monitor {name} doesn't exists.");
            }
            return res;
        }

        [HttpDelete("{name}")]
        public ActionResult DeleteMonitor(string name)
        {
            _logger.LogDebug($"Delete monitor {name}.");
            _monitorManager.Tell(new DeleteActorMessage(name.GetHashCode()));
            return NoContent();
        }
    }
}
