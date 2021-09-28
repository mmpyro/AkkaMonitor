using Monitor.Enums;

namespace Monitor.Messages
{
    public abstract class CreateAlertMessage {
        public CreateAlertMessage(string name)
        {
            Name = $"{name}-{Type}";
        }

        public string Name { get; }

        public abstract AlertType Type { get; }

        public override bool Equals(object obj)
        {
            var message = obj as CreateAlertMessage;
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

    public class CreateSlackAlertMessage : CreateAlertMessage
    {
        public CreateSlackAlertMessage(string name, string url, string channel) : base(name)
        {
            Url = url;
            Channel = channel;
        }

        public string Url { get;}
        public string Channel { get;}

        public override AlertType Type => AlertType.Slack;
    }
}