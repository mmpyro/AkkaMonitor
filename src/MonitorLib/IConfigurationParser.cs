using MonitorLib.Messages;
using System.Collections.Generic;

namespace MonitorLib
{
    public interface IConfigurationParser
    {
        IEnumerable<CreateMonitorMessageReq> Monitors { get; }
        IEnumerable<CreateAlertMessageReq> Alerts { get; }
    }
}