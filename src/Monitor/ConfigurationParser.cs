using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MonitorLib;
using MonitorLib.Enums;
using MonitorLib.Messages;
using System;
using System.Collections.Generic;

namespace Monitor
{
    public class ConfigurationParser : IConfigurationParser
    {
        private const int DEFAULT_INTERVAL = 15;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConfigurationParser> _logger;

        public ConfigurationParser(IConfiguration configuration, ILogger<ConfigurationParser> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private int CheckInterval(string interval)
        {
            if (int.TryParse(interval, out int checkInterval))
            {
                return checkInterval > 0 ? checkInterval : DEFAULT_INTERVAL;
            }
            return DEFAULT_INTERVAL;
        }

        private MonitorMode GetMode(string mode)
        {
            var res = Enum.Parse<MonitorMode>(mode);
            return res;
        }

        public IEnumerable<CreateMonitorMessageReq> Monitors
        {
            get
            {
                var monitors = _configuration.GetSection("Monitors");
                foreach (var monitor in monitors.GetChildren())
                {
                    switch (monitor["Type"])
                    {
                        case "Http":
                            int expectedStatusCode = int.Parse(monitor["ExpectedStatusCode"]);
                            yield return new CreateHttpMonitorMessage(monitor["Name"], monitor["Url"], expectedStatusCode, CheckInterval(monitor["Interval"]), GetMode(monitor["Mode"]));
                            break;
                        case "DNS":
                            yield return new CreateDnsMonitorMessage(monitor["Name"], monitor["Hostname"], CheckInterval(monitor["Interval"]), GetMode(monitor["Mode"]));
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public IEnumerable<CreateAlertMessageReq> Alerts
        {
            get
            {
                var alerts = _configuration.GetSection("Alerts");
                foreach (var alert in alerts.GetChildren())
                {
                    switch (alert["Type"])
                    {
                        case "Slack":
                            yield return new CreateSlackAlertMessage(alert["Name"], alert["Url"], alert["Channel"]);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}