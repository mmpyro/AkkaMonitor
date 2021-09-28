using Monitor.Messages;
using System.Collections.Generic;

namespace Monitor
{
    public interface IConfigurationParser
    {
        IEnumerable<CreateMonitorMessage> Monitors { get; }
        IEnumerable<CreateAlertMessage> Alerts { get; }
    }
}