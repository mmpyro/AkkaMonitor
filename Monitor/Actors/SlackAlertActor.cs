using System;
using Akka.Actor;
using Monitor.Clients;
using Monitor.Factories;
using Monitor.Messages;

namespace Monitor.Actors
{
    public class SlackAlertActor : MetricActor
    {
        private const string SLACK_USER = "webhookbot";
        private readonly ISlackClient _slackClient;
        
        public SlackAlertActor(CreateSlackAlertMessage message, ISlackClientFactory slackClientFactory)
        {
            try
            {
                _slackClient = slackClientFactory.Create(message.Url);
            }
            catch(UriFormatException ex)
            {
                Log.Error(ex, $"Exception during {nameof(SlackAlertActor)} actor creation. Invalid slack url. Actor is stoped.");
                Context.Parent.Tell(new DeleteActorMessage(message.GetHashCode()));
            }

            Receive<TriggerAlertMessage>(m => {
                _slackClient.PostMessage(m.Content, SLACK_USER, message.Channel);
            });

            Receive<TriggerAlertCancelationMessage>(m => {
                _slackClient.PostMessage(m.Content, SLACK_USER, message.Channel);
            });
        }

        public static Props Props(CreateSlackAlertMessage message, ISlackClientFactory slackClientFactory)
        {
            return Akka.Actor.Props.Create(() => new SlackAlertActor(message, slackClientFactory));
        }
    }
}