using Monitor.Enums;

namespace Monitor.Messages
{
    public class ActiveActorMetricMessage {}

    public class InactiveActorMetricMessage {}

    public class MetricMessage
    {
        public MetricMessage(string name, MonitorType type, string identifier)
        {
            Name = name;
            Type = type;
            Identifier = identifier;
        }

        public string Name { get; private set; }
        public MonitorType Type { get; private set; }
        public string Identifier { get; private set; }
        public virtual string[] Labels => new [] {Name, Type.ToString(), Identifier};
    }

    public class UpMonitorMetricMessage : MetricMessage
    {
        public UpMonitorMetricMessage(string name, MonitorType type, string identifier, MonitorState state) : base(name, type, identifier)
        {
            State = state;
        }

        public MonitorState State { get; private set; }
    }

    public class MonitorLattencyMessage : MetricMessage
    {
        public MonitorLattencyMessage(string name, MonitorType type, string identifier, double  value) : base(name, type, identifier)
        {
            Value = value;
        }

        public double Value { get; private set; }
    }
}