using Microsoft.Extensions.Configuration;
using Monitor.Messages;
using System.Collections.Generic;

namespace Monitor
{
    public interface IConfigurationParser
    {
        IEnumerable<CreateMonitorMessage> Monitors { get; }
        IEnumerable<CreateAlertMessage> Alerts { get; }
    }

    public class ConfigurationParser : IConfigurationParser
    {
        private const int DEFAULT_INTERVAL = 15;
        private readonly IConfigurationRoot _configuration;
        public ConfigurationParser(IConfigurationRoot configuration)
        {
            _configuration = configuration;
        }

        private int CheckInterval(string interval)
        {
            if (int.TryParse(interval, out int checkInterval))
            {
                return checkInterval > 0 ? checkInterval : DEFAULT_INTERVAL;
            }
            return DEFAULT_INTERVAL;
        }

        public IEnumerable<CreateMonitorMessage> Monitors
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
                            yield return new CreateHttpMonitorMessage(monitor["Url"], expectedStatusCode, CheckInterval(monitor["CheckInterval"]));
                            break;
                        case "DNS":
                            yield return new CreateDnsMonitorMessage(monitor["Hostname"], CheckInterval(monitor["CheckInterval"]));
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public IEnumerable<CreateAlertMessage> Alerts
        {
            get
            {
                var alerts = _configuration.GetSection("Alerts");
                foreach (var alert in alerts.GetChildren())
                {
                    switch (alert["Type"])
                    {
                        case "Slack":
                            yield return new CreateSlackAlertMessage(alert["SlackUrl"], alert["SlackChannel"]);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}