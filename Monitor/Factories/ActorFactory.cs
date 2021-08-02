using System;
using Akka.Actor;
using Monitor.Actors;
using Monitor.Dtos;
using Monitor.Messages;

namespace Monitor.Factories
{
    public interface IActorFactory
    {
        Props CreateMonitorActor(CreateMonitorMessage message);
        Props CreateAlertActor(CreateAlertMessage message);
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

        public Props CreateMonitorActor(CreateMonitorMessage message)
        {
            switch(message)
            {
                case CreateHttpMonitorMessage:
                    return HttpMonitorActor.Props(RequestParameters.From((CreateHttpMonitorMessage) message), _requestFactory.Create());
                default:
                    throw new ArgumentException($"Not supported message type: {message.GetType().Name}");
            }
        }

        public Props CreateAlertActor(CreateAlertMessage message)
        {
            switch(message)
            {
                case CreateSlackAlertMessage:
                    return SlackAlertActor.Props(SlackConfiguration.From((CreateSlackAlertMessage) message), _slackClientFactory);
                default:
                    throw new ArgumentException($"Not supported message type: {message.GetType().Name}");
            }
        }
    }
}