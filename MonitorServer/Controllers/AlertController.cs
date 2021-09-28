using Akka.Actor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Monitor.Actors;
using Monitor.Messages;
using System.Collections.Generic;
using System.Threading.Tasks;
using static MonitorServer.Startup;

namespace MonitorServer.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AlertController : ControllerBase
    {
        private readonly ILogger<AlertController> _logger;
        private readonly IActorRef _alertManager;

        public AlertController(AlertManagerActorProvider alertManager, ILogger<AlertController> logger)
        {
            _alertManager = alertManager();
            _logger = logger;
        }

        [HttpDelete("{name}")]
        public ActionResult DeleteAlert(string name)
        {
            _logger.LogDebug($"Delete alert {name}.");
            _alertManager.Tell(new DeleteActorMessage(name.GetHashCode()));
            return NoContent();
        }

        [HttpGet]
        public async Task<IEnumerable<AlertInfo>> Get()
        {
            _logger.LogDebug("Get alerts");
            var res = await _alertManager.Ask<ListAlertMessageRes>(new ListAlertMessageReq());
            return res.Alerts;
        }

        [HttpGet("{name}")]
        public async Task<ActionResult<AlertDetailsMessageRes>> GetAlertInfo(string name)
        {
            _logger.LogDebug($"Get alert details for: {name}");
            var res = await _alertManager.Ask<AlertDetailsMessageRes>(new AlertDetailsMessageReq(name));
            if (res == null)
            {
                return NotFound($"Alert {name} doesn't exists.");
            }
            return res;
        }
    }
}
