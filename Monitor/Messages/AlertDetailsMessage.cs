using Monitor.Enums;

namespace Monitor.Messages
{
    public class AlertDetailsMessageReq
    {
        public AlertDetailsMessageReq(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    public class AlertDetailsMessageRes
    {
        public AlertDetailsMessageRes(string name, AlertType type, object parameters = null)
        {
            Name = name;
            Type = type;
            Parameters = parameters;
        }

        public string Name { get; }
        public AlertType Type { get; }

        public object Parameters { get; }
    }
}
