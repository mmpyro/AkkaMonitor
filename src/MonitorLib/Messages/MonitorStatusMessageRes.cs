using MonitorLib.Enums;

namespace MonitorLib.Messages
{

    public class MonitorStatusMessageRes
    {
        public MonitorStatusMessageRes(string name, int interval, string identifier, MonitorType type, MonitorMode mode, MonitorState state = MonitorState.Unknown)
        {
            Name = name;
            Interval = interval;
            Identifier = identifier;
            Type = type;
            State = state;
            Mode = mode;
        }

        public MonitorState State { get; }
        public string Name { get; }
        public int Interval { get; }
        public string Identifier { get; }
        public MonitorMode Mode { get; }
        public MonitorType Type { get; }
    }
}