using System;
using Akka.Actor;
using Monitor.Clients;
using Monitor.Factories;
using Monitor.Messages;
using InitParameters = Monitor.Messages.CreateSlackAlertMessage;

namespace Monitor.Actors
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
                    _slackClient.PostMessage(m.Content, SLACK_USER, parameters.Channel);
                });

                Receive<TriggerAlertCancelationMessage>(m => {
                    _slackClient.PostMessage(m.Content, SLACK_USER, parameters.Channel);
                });
            }
            catch(UriFormatException ex)
            {
                Log.Error(ex, $"Exception during {nameof(SlackAlertActor)} actor creation. Invalid slack url. Actor is stoped.");
                Context.Parent.Tell(new DeleteActorMessage(parameters.GetHashCode()));
            }
        }

        public static Props Props(InitParameters parameters, ISlackClientFactory slackClientFactory)
        {
            return Akka.Actor.Props.Create(() => new SlackAlertActor(parameters, slackClientFactory));
        }
    }
}