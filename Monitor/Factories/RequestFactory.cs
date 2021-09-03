using Monitor.Clients;

namespace Monitor.Factories
{
    public interface IRequestFactory
    {
        IRequest CreateRequestClient();
        IDnsRequest CreateDnsRequestClient();
    }

    public class RequestFactory : IRequestFactory
    {
        public IDnsRequest CreateDnsRequestClient()
        {
            return new DnsRequest();
        }

        public IRequest CreateRequestClient()
        {
            return new Request();
        }
    }
}