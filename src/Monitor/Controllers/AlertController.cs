using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Monitor;
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
        private readonly IMonitorController _monitorController;
        private readonly IMapper _mapper;

        public AlertController(IMonitorController monitorController, IMapper mapper, ILogger<AlertController> logger)
        {
            _monitorController = monitorController;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpDelete("{name}")]
        public ActionResult DeleteAlert(string name)
        {
            _logger.LogDebug($"Delete alert {name}.");
            _monitorController.DeleteAlert(name);
            return NoContent();
        }

        [HttpGet]
        public async Task<IEnumerable<AlertInfo>> Get()
        {
            _logger.LogDebug("Get alerts");
            var res = await _monitorController.ListAlerts();
            return res;
        }

        [HttpGet("{name}")]
        public async Task<ActionResult<AlertDetails>> GetAlertInfo(string name)
        {
            _logger.LogDebug($"Get alert details for: {name}");
            var res = await _monitorController.GetAlertInfo(name);
            if (res == null)
            {
                return NotFound($"Alert {name} doesn't exists.");
            }
            return _mapper.Map<AlertDetailsMessageRes, AlertDetails>(res);
        }

        [HttpPost("slack")]
        public async Task<ActionResult> CreateAlert([FromBody] SlackAlert data)
        {
            try
            {
                var msg = _mapper.Map<SlackAlert, CreateSlackAlertMessage>(data);
                var res = await _monitorController.CreateAlert(msg);
                if (res != null)
                    return Ok($"Alert {res.Name} created.");
                return BadRequest($"Alert wasn't created for request {data}.");
            }
            catch(AutoMapperMappingException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
