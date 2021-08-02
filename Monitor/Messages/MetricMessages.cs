using Monitor.Enums;

namespace Monitor.Messages
{
    public class ActiveActorMetricMessage {}

    public class InactiveActorMetricMessage {}

    public class UpMonitorMetricMessage
    {
        public string Name { get; set; }
        public MonitorState State { get; set; }
        public MonitorType Type { get; set; }
        public string Identifier { get; set; }
    }

    public class MonitorLattencyMessage
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public MonitorType Type { get; set; }
        public string Identifier { get; set; }
    }
}