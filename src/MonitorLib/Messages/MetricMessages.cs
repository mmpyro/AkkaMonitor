using MonitorLib.Enums;

namespace MonitorLib.Messages
{
    public record ActiveActorMetricMessage;

    public record InactiveActorMetricMessage;

    public abstract record MetricMessage(string Name, MonitorType Type, string Identifier)
    {
        public virtual string[] Labels => new [] {Name, Type.ToString(), Identifier};
    }

    public record UpMonitorMetricMessage(string Name, MonitorType Type, string Identifier, MonitorState State) 
        : MetricMessage(Name, Type, Identifier);

    public record MonitorLattencyMessage(string Name, MonitorType Type, string Identifier, double Value) 
        : MetricMessage(Name, Type, Identifier);
}