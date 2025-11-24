using System.Collections.Generic;
using MonitorLib.Dtos;

namespace MonitorLib.Messages
{
    public record ListMonitorMessageRes(IEnumerable<MonitorInfo> Monitors);
}