using System.Collections.ObjectModel;
using Monitor.Actors;

namespace Monitor.Messages
{
    public class ListMonitorMessageReq {}

    public class ListMonitorMessageRes
    {
        public ListMonitorMessageRes(ReadOnlyCollection<MonitorInfo> monitors)
        {
            Monitors = monitors; 
        }
        public ReadOnlyCollection<MonitorInfo> Monitors { get; }
    }
}