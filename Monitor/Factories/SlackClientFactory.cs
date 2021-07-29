using Monitor.Clients;

namespace Monitor.Factories
{
    public interface ISlackClientFactory
    {
        ISlackClient Create(string urlWithAccessToken);
    }

    public class SlackClientFactory : ISlackClientFactory
    {
        public ISlackClient Create(string urlWithAccessToken)
        {
            return new SlackClient(urlWithAccessToken);
        }
    }
}