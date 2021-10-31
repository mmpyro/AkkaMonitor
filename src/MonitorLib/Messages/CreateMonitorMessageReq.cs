using MonitorLib.Enums;

namespace MonitorLib.Messages
{

    public abstract class CreateMonitorMessageReq {
        public int Interval { get; }

        public int Id => GetHashCode();

        public string Name { get; }  

        public string Identifier { get; }

        public MonitorMode Mode { get; }

        public abstract MonitorType Type { get; }

        public CreateMonitorMessageReq(string name, string identifier, int interval, MonitorMode mode)
        {
            Interval = interval;
            Identifier = identifier;
            Name = $"{name}-{Type}";
            Mode = mode;
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
            var monitor = obj as CreateMonitorMessageReq;
            return monitor != null ? monitor.Name == Name : false;
        }
    }

    public class CreateDnsMonitorMessage : CreateMonitorMessageReq
    {
        public CreateDnsMonitorMessage(string name, string hostname, int interval, MonitorMode mode = MonitorMode.Poke) : base(name, hostname, interval, mode)
        {
        }

        public override MonitorType Type => MonitorType.DNS;
    }

    public class CreateHttpMonitorMessage : CreateMonitorMessageReq
    {
        public CreateHttpMonitorMessage(string name, string url, int expectedStatusCode, int interval, MonitorMode mode = MonitorMode.Poke) : base(name, url, interval, mode)
        {
            ExpectedStatusCode = expectedStatusCode;
        }

        public int ExpectedStatusCode { get; set; }

        public override MonitorType Type => MonitorType.Http;
    }
}