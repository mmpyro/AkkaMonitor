using Monitor.Enums;

namespace Monitor.Messages
{
    public class ActiveActorMetricMessage {}

    public class InactiveActorMetricMessage {}

    public class MetricMessage
    {
        public string Name { get; set; }
        public MonitorType Type { get; set; }
        public string Identifier { get; set; }
        public virtual string[] Labels => new [] {Name, Type.ToString(), Identifier};
    }

    public class UpMonitorMetricMessage : MetricMessage
    {
        public MonitorState State { get; set; }
    }

    public class MonitorLattencyMessage : MetricMessage
    {
        public double Value { get; set; }
    }
}