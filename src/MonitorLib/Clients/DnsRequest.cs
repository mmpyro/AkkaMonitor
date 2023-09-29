using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MonitorLib.Clients
{
    public interface IDnsRequest
    {
        Task<bool> GetHostEntry(string hostname);
    }

    public class DnsRequest : IDnsRequest
    {
        public async Task<bool> GetHostEntry(string hostname)
        {
            try
            {
                var ipHostEntry = await Dns.GetHostEntryAsync(hostname);
                return ipHostEntry.AddressList.Any();
            }
            catch(Exception ex)
            {
                throw new GetHostEntryException($"Cannot resolve DNS hostname: {hostname}.", ex);
            }
        }
    }

    public class GetHostEntryException : Exception
    {
        public GetHostEntryException(string message, Exception innerException) : base(message, innerException) {}
    }
}