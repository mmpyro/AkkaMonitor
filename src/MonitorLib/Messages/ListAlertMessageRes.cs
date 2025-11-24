using MonitorLib.Dtos;
using System.Collections.Generic;

namespace MonitorLib.Messages
{
    public record ListAlertMessageRes(IEnumerable<AlertInfo> Alerts);
}
