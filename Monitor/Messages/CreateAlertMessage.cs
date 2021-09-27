using System;

namespace Monitor.Messages
{
    public class CreateAlertMessage {}

    public class CreateSlackAlertMessage : CreateAlertMessage
    {
        public CreateSlackAlertMessage(string url, string channel)
        {
            Url = url;
            Channel = channel;
        }

        public string Url { get;}
        public string Channel { get;}

        public override bool Equals(object obj)
        {
            var message = obj as CreateSlackAlertMessage;
            if(message != null)
            {
                return message.Channel == Channel && message.Url == Url;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Url.GetHashCode(),Channel.GetHashCode());
        }
    }
}