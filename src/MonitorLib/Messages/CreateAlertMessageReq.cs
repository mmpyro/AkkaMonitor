using MonitorLib.Enums;

namespace MonitorLib.Messages
{

    public abstract class CreateAlertMessageReq
    {
        public CreateAlertMessageReq(string name)
        {
            Name = $"{name}-{Type}";
        }

        public string Name { get; }

        public abstract AlertType Type { get; }

        public override bool Equals(object obj)
        {
            var message = obj as CreateAlertMessageReq;
            if (message != null)
            {
                return message.Name == Name;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }

    public class CreateSlackAlertMessage : CreateAlertMessageReq
    {
        public CreateSlackAlertMessage(string name, string url, string channel) : base(name)
        {
            Url = url;
            Channel = channel;
        }

        public string Url { get; }
        public string Channel { get; }

        public override AlertType Type => AlertType.Slack;
    }
}