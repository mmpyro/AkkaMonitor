using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
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
        private readonly IMonitorController _monitorController;
        private readonly IMonitorCreationValidator _validator;
        private readonly IMapper _mapper;

        public MonitorController(IMonitorController monitorController, IMonitorCreationValidator validator, IMapper mapper, ILogger<MonitorController> logger)
        {
            _monitorController = monitorController;
            _validator = validator;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IEnumerable<MonitorInfo>> Get()
        {
            _logger.LogDebug("Get monitors");
            var res = await _monitorController.ListMonitors();
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
            _monitorController.DeleteMonitor(name);
            return NoContent();
        }

        [HttpPost("http")]
        public async Task<ActionResult> CreateHttpMonitor([FromBody] HttpMonitor data)
        {
            return await CreateMonitor<CreateHttpMonitorMessage>(data);
        }

        [HttpPost("dns")]
        public async Task<ActionResult> CreateDnsMonitor([FromBody] DnsMonitor data)
        {
            return await CreateMonitor<CreateDnsMonitorMessage>(data);
        }

        private async Task<ActionResult> CreateMonitor<T>(object data) where T : CreateMonitorMessageReq
        {
            try
            {
                var msg = _mapper.Map<T>(data);
                _validator.Validate(msg);
                var res = await _monitorController.CreateMonitor(msg);
                if (res != null)
                    return Ok($"Monitor {res.Name} created.");
                return BadRequest($"Monitor wasn't created for request {data}.");
            }
            catch(ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch(AutoMapperMappingException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
