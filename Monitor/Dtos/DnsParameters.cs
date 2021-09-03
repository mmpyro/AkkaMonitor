using Monitor.Messages;

namespace Monitor.Dtos
{
    public class DnsParameters
    {
        public string Hostname { get; set; }

        public DnsParameters()
        {
            
        }
        public DnsParameters(string hostName)
        {
            Hostname = hostName;
        }

        public static DnsParameters From(CreateDnsMonitorMessage message)
        {
            return new DnsParameters(message.Hostname);
        }
    }
}