using Monitor.Enums;

namespace Monitor.Messages
{
    public class MonitorStatusMessageReq
    {
        public MonitorStatusMessageReq(string name)
        {
            Name = name;
        }
        public string Name { get; }
    }

    public class MonitorStatusMessageRes
    {
        public MonitorStatusMessageRes(string name, int interval, string identifier, MonitorType type, MonitorState state = MonitorState.Unknown)
        {
            Name = name;
            Interval = interval;
            Identifier = identifier;
            Type = type;
            State = state;
        }

        public MonitorState State { get; }
        public string Name { get; }
        public int Interval { get; }
        public string Identifier { get; }
        public MonitorType Type { get; }
    }
}