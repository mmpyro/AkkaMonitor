using System;
using Akka.Actor;
using MonitorLib.Actors;
using MonitorLib.Messages;

namespace MonitorLib.Factories
{
    public interface IActorFactory
    {
        Props CreateMonitorActor(CreateMonitorMessageReq message);
        Props CreateAlertActor(CreateAlertMessageReq message);
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

        public Props CreateMonitorActor(CreateMonitorMessageReq message)
        {
            switch(message)
            {
                case CreateHttpMonitorMessage:
                    return HttpMonitorActor.Props((CreateHttpMonitorMessage) message, _requestFactory.CreateRequestClient());
                case CreateDnsMonitorMessage:
                    return DnsMonitorActor.Props((CreateDnsMonitorMessage) message, _requestFactory.CreateDnsRequestClient());
                default:
                    throw new ArgumentException($"Not supported message type: {message.GetType().Name}");
            }
        }

        public Props CreateAlertActor(CreateAlertMessageReq message)
        {
            switch(message)
            {
                case CreateSlackAlertMessage:
                    return SlackAlertActor.Props((CreateSlackAlertMessage) message, _slackClientFactory);
                default:
                    throw new ArgumentException($"Not supported message type: {message.GetType().Name}");
            }
        }
    }
}