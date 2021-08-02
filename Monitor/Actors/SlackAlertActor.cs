using Akka.Actor;
using Monitor.Clients;
using Monitor.Dtos;
using Monitor.Factories;
using Monitor.Messages;

namespace Monitor.Actors
{
    public class SlackAlertActor : MetricActor
    {
        private const string SLACK_USER = "webhookbot";
        private readonly ISlackClient _slackClient;
        
        public SlackAlertActor(SlackConfiguration configuration, ISlackClientFactory slackClientFactory)
        {
            _slackClient = slackClientFactory.Create(configuration.Url);

            Receive<TriggerAlertMessage>(m => {
                _slackClient.PostMessage(m.Content, SLACK_USER, configuration.Channel);
            });

            Receive<TriggerAlertCancelationMessage>(m => {
                _slackClient.PostMessage(m.Content, SLACK_USER, configuration.Channel);
            });
        }

        public static Props Props(SlackConfiguration configuration, ISlackClientFactory slackClientFactory)
        {
            return Akka.Actor.Props.Create(() => new SlackAlertActor(configuration, slackClientFactory));
        }
    }
}