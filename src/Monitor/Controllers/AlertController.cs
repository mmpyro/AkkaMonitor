using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Monitor;
using MonitorLib.Actors;
using MonitorLib.Dtos;
using MonitorLib.Messages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonitorServer.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AlertController : ControllerBase
    {
        private readonly ILogger<AlertController> _logger;
        private readonly IMonitorController _monitorManager;

        public AlertController(IMonitorController monitorManager, ILogger<AlertController> logger)
        {
            _monitorManager = monitorManager;
            _logger = logger;
        }

        [HttpDelete("{name}")]
        public ActionResult DeleteAlert(string name)
        {
            _logger.LogDebug($"Delete alert {name}.");
            _monitorManager.DeleteAlert(name);
            return NoContent();
        }

        [HttpGet]
        public async Task<IEnumerable<AlertInfo>> Get()
        {
            _logger.LogDebug("Get alerts");
            var res = await _monitorManager.ListAlerts();
            return res;
        }

        [HttpGet("{name}")]
        public async Task<ActionResult<AlertDetailsMessageRes>> GetAlertInfo(string name)
        {
            _logger.LogDebug($"Get alert details for: {name}");
            var res = await _monitorManager.GetAlertInfo(name);
            if (res == null)
            {
                return NotFound($"Alert {name} doesn't exists.");
            }
            return res;
        }

        [HttpPost("slack")]
        public async Task<ActionResult> CreateAlert([FromBody] CreateSlackAlertMessage data)
        {
            var res = await _monitorManager.CreateAlert(data);
            if (res != null)
                return Ok($"Alert {res.Name} created.");
            return BadRequest($"Alert wasn't created for request {data}.");
        }
    }
}
