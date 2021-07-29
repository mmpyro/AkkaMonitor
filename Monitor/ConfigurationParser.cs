using Microsoft.Extensions.Configuration;
using Monitor.Messages;
using System.Collections.Generic;

namespace Monitor
{
    public interface IConfigurationParser
    {
        int CheckInterval { get; }
        IEnumerable<CreateMonitorMessage> Monitors { get; }
        IEnumerable<CreateAlertMessage> Alerts { get; }
    }

    public class ConfigurationParser : IConfigurationParser
    {
        private readonly IConfigurationRoot _configuration;
        public ConfigurationParser(IConfigurationRoot configuration)
        {
            _configuration = configuration;
        }

        public int CheckInterval
        {
            get
            {
                var interval = _configuration["CheckInterval"];
                if (int.TryParse(interval, out int checkInterval))
                {
                    return checkInterval;
                }
                return 15;
            }
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
                            yield return new CreateHttpMonitorMessage(monitor["Url"], expectedStatusCode);
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