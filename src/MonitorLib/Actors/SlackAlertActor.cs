using System;
using Akka.Actor;
using MonitorLib.Clients;
using MonitorLib.Factories;
using MonitorLib.Messages;
using InitParameters = MonitorLib.Messages.CreateSlackAlertMessage;

namespace MonitorLib.Actors
{
    public class SlackAlertActor : MetricActor
    {
        private const string SLACK_USER = "webhookbot";
        private readonly ISlackClient _slackClient;
        
        public SlackAlertActor(InitParameters parameters, ISlackClientFactory slackClientFactory)
        {
            try
            {
                _slackClient = slackClientFactory.Create(parameters.Url);

                Receive<TriggerAlertMessage>(m => {
                    Log.Info($"Sending TriggerAlertMessage");
                    _slackClient.PostMessage(m.Content, SLACK_USER, parameters.Channel);
                });

                Receive<TriggerAlertCancelationMessage>(m => {
                    Log.Info($"Sending TriggerAlertCancelationMessage");
                    _slackClient.PostMessage(m.Content, SLACK_USER, parameters.Channel);
                });

                Receive<AlertDetailsMessageReq>(m => {
                    Sender.Tell(new AlertDetailsMessageRes(parameters.Name, parameters.Type, new { Url = parameters.Url, Channel = parameters.Channel}));
                });
            }
            catch(UriFormatException ex)
            {
                Log.Info(ex, $"Become Failed. Exception during {nameof(SlackAlertActor)} actor creation. Invalid slack url. Actor is stopped.");
                Context.Parent.Tell(new DeleteActorMessage(parameters.GetHashCode()));
            }
        }

        public static Props Props(InitParameters parameters, ISlackClientFactory slackClientFactory)
        {
            return Akka.Actor.Props.Create(() => new SlackAlertActor(parameters, slackClientFactory));
        }
    }
}