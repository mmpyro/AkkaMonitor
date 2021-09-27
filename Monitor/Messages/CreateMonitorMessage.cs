using Monitor.Enums;

namespace Monitor.Messages
{
    public abstract class CreateMonitorMessage {
        public int CheckInterval { get; }

        public int Id => GetHashCode();

        public string Name { get; }  

        public string Identifier { get; }

        public abstract MonitorType Type { get; }

        public CreateMonitorMessage(string name, string identifier, int checkInterval)
        {
            CheckInterval = checkInterval;
            Identifier = identifier;
            Name = $"{name}-{Type}";
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            var monitor = obj as CreateMonitorMessage;
            return monitor != null ? monitor.Name == Name : false;
        }
    }

    public class CreateDnsMonitorMessage : CreateMonitorMessage
    {
        public CreateDnsMonitorMessage(string name, string hostName, int checkInterval) : base(name, hostName, checkInterval)
        {
        }

        public override MonitorType Type => MonitorType.DNS;
    }

    public class CreateHttpMonitorMessage : CreateMonitorMessage
    {
        public CreateHttpMonitorMessage(string name, string url, int expectedStatusCode, int checkInterval) : base(name, url, checkInterval)
        {
            ExpectedStatusCode = expectedStatusCode;
        }

        public int ExpectedStatusCode { get; set; }

        public override MonitorType Type => MonitorType.Http;
    }
}