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
                    var m = message as CreateHttpMonitorMessage;
                    return Akka.Actor.Props.Create(() => new HttpMonitorActor(RequestParameters.From(m), _requestFactory.Create()));
                default:
                    throw new ArgumentException($"Not supported message type: {message.GetType().Name}");
            }
        }

        public Props CreateAlertActor(CreateAlertMessage message)
        {
            switch(message)
            {
                case CreateSlackAlertMessage:
                    var m = message as CreateSlackAlertMessage;
                    return Akka.Actor.Props.Create(() => new SlackAlertActor(SlackConfiguration.From(m), _slackClientFactory));
                default:
                    throw new ArgumentException($"Not supported message type: {message.GetType().Name}");
            }
        }
    }
}