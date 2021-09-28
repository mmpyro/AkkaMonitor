using Monitor.Actors;
using System.Collections.ObjectModel;

namespace Monitor.Messages
{
    public class ListAlertMessageReq { }

    public class ListAlertMessageRes
    {
        public ListAlertMessageRes(ReadOnlyCollection<AlertInfo> alerts)
        {
            Alerts = alerts;
        }

        public ReadOnlyCollection<AlertInfo> Alerts { get; }
    }
}
