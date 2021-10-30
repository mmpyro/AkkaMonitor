using System.Collections.ObjectModel;
using MonitorLib.Dtos;

namespace MonitorLib.Messages
{

    public class ListMonitorMessageRes
    {
        public ListMonitorMessageRes(ReadOnlyCollection<MonitorInfo> monitors)
        {
            Monitors = monitors; 
        }
        public ReadOnlyCollection<MonitorInfo> Monitors { get; }
    }
}