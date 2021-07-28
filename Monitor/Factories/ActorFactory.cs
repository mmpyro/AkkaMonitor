using Akka.Actor;
using Monitor.Actors;
using Monitor.Dtos;

namespace Monitor.Factories
{
    public interface IActorFactory
    {
        Props CreateHttpMonitorActor(RequestParameters requestParameters);
        Props CreateSlackAlertActor(SlackConfiguration slackConfiguration);
    }

    public class ActorFactory : IActorFactory
    {
        private readonly IRequestFactory _requestFactory;
        private readonly ISlackClientFactory _slackClientFactory;

        public ActorFactory(IRequestFactory requestFactory, ISlackClientFactory slackClientFactory)
        {
            _slackClientFactory = slackClientFactory;
            _requestFactory = requestFactory;
        }

        public Props CreateHttpMonitorActor(RequestParameters requestParameters)
        {
            return Akka.Actor.Props.Create(() => new HttpMonitorActor(requestParameters, _requestFactory.Create()));
        }

        public Props CreateSlackAlertActor(SlackConfiguration slackConfiguration)
        {
            return Akka.Actor.Props.Create(() => new SlackAlertActor(slackConfiguration, _slackClientFactory));
        }
    }
}