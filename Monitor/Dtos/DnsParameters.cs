using Monitor.Messages;

namespace Monitor.Dtos
{
    public class DnsParameters
    {
        public string Hostname { get; set; }
        public int CheckInterval { get; set; }

        public DnsParameters()
        {
            
        }
        public DnsParameters(string hostName, int checkInterval)
        {
            Hostname = hostName;
            CheckInterval = checkInterval;
        }

        public static DnsParameters From(CreateDnsMonitorMessage message)
        {
            return new DnsParameters(message.Hostname, message.CheckInterval);
        }
    }
}