namespace Monitor.Messages
{
    public class CreateMonitorMessage {
        public int CheckInterval { get; protected set; }
    }

    public class CreateDnsMonitorMessage : CreateMonitorMessage
    {
        public CreateDnsMonitorMessage(string hostName, int checkInterval)
        {
            Hostname = hostName;
            CheckInterval = checkInterval;
        }

        public string Hostname { get; private set; }

        public override int GetHashCode()
        {
            return Hostname.GetHashCode();
        }

        public override string ToString()
        {
            return Hostname;
        }

        public override bool Equals(object obj)
        {
            var message = obj as CreateDnsMonitorMessage;
            if(message != null)
            {
                return message.Hostname == Hostname;
            }
            return false;
        }
    }

    public class CreateHttpMonitorMessage : CreateMonitorMessage
    {
        public CreateHttpMonitorMessage(string url, int expectedStatusCode, int checkInterval)
        {
            Url = url;
            ExpectedStatusCode = expectedStatusCode;
            CheckInterval = checkInterval;
        }

        public string Url { get; private set; }

        public int ExpectedStatusCode { get; set; }

        public override bool Equals(object obj)
        {
            var message = obj as CreateHttpMonitorMessage;
            if(message != null)
            {
                return message.Url == Url;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Url.GetHashCode();
        }

        public override string ToString()
        {
            return Url;
        }
    }
}