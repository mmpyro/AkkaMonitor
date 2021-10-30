using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Monitor;
using MonitorLib.Dtos;
using MonitorLib.Messages;
using MonitorLib.Validators;

namespace MonitorServer.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class MonitorController : ControllerBase
    {
        private readonly ILogger<MonitorController> _logger;
        private readonly IMonitorController _monitorManager;
        private readonly IMonitorCreationValidator _validator;

        public MonitorController(IMonitorController monitorManager, IMonitorCreationValidator validator, ILogger<MonitorController> logger)
        {
            _monitorManager = monitorManager;
            _validator = validator;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IEnumerable<MonitorInfo>> Get()
        {
            _logger.LogDebug("Get monitors");
            var res = await _monitorManager.ListMonitors();
            return res;
        }

        //[HttpGet("{name}")]
        //public async Task<ActionResult<MonitorStatusMessageRes>> GetMonitorInfo(string name)
        //{
        //    _logger.LogDebug($"Get monitor status for: {name}");
        //    var res = await _monitorManager.GetMonitorInfo(name);
        //    if(res == null)
        //    {
        //        return NotFound($"Monitor {name} doesn't exists.");
        //    }
        //    return res;
        //}

        [HttpDelete("{name}")]
        public ActionResult DeleteMonitor(string name)
        {
            _logger.LogDebug($"Delete monitor {name}.");
            _monitorManager.DeleteMonitor(name);
            return NoContent();
        }

        [HttpPost("http")]
        public async Task<ActionResult> CreateHttpMonitor([FromBody] CreateHttpMonitorMessage data)
        {
            return await CreateMonitor(data);
        }

        [HttpPost("dns")]
        public async Task<ActionResult> CreateDnsMonitor([FromBody] CreateDnsMonitorMessage data)
        {
            return await CreateMonitor(data);
        }

        private async Task<ActionResult> CreateMonitor(CreateMonitorMessageReq data)
        {
            try
            {
                _validator.Validate(data);
                var res = await _monitorManager.CreateMonitor(data);
                if (res != null)
                    return Ok($"Monitor {res.Name} created.");
                return BadRequest($"Monitor wasn't created for request {data}.");
            }
            catch(ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
