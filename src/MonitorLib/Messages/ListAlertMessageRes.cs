using MonitorLib.Actors;
using MonitorLib.Dtos;
using System.Collections.ObjectModel;

namespace MonitorLib.Messages
{

    public class ListAlertMessageRes
    {
        public ListAlertMessageRes(ReadOnlyCollection<AlertInfo> alerts)
        {
            Alerts = alerts;
        }

        public ReadOnlyCollection<AlertInfo> Alerts { get; }
    }
}
