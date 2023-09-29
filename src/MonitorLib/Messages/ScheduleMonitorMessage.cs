namespace MonitorLib.Messages
{
    public class ScheduleMonitorMessage
    {
        public ScheduleMonitorMessage(CreateMonitorMessageReq monitorMessageReq)
        {
            MonitorMessageReq = monitorMessageReq;
        }

        public CreateMonitorMessageReq MonitorMessageReq { get; }
    }
}
