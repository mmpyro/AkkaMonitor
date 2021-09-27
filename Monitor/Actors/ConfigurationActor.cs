

using Akka.Actor;
using Monitor.Extensions;
using Monitor.Messages;

namespace Monitor.Actors
{
    public class ConfigurationActor : MetricActor
    {
        private readonly IConfigurationParser _configurationParser;
        public ConfigurationActor(IConfigurationParser configurationParser)
        {
            _configurationParser = configurationParser;

            Receive<LoadConfigurationMessage>(_ => {
                _configurationParser.Monitors.Each( m => {
                    var selection = Context.System.ActorSelection(MONITOR_MANAGER);
                    selection.Tell(m);
                });
                _configurationParser.Alerts.Each(a => {
                    var selection = Context.System.ActorSelection(ALERT_MANAGER);
                    selection.Tell(a);
                });
            });
        }

        protected override void PreStart()
        {
            base.PreStart();
            Self.Tell(new LoadConfigurationMessage(), Self);
        }

        public static Props Create(IConfigurationParser configurationParser)
        {
            return Props.Create(() => new ConfigurationActor(configurationParser));
        }
    }
}