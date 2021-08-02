using Akka.Event;
using Monitor.Enums;
using Monitor.Messages;
using Prometheus;

namespace Monitor.Actors
{
    public class PrometheusMetricActor : MetricActor
    {
        private readonly string[] _monitor_labels = new string []{"monitor", "type", "identifier"};
        private readonly Gauge _activeActors = Metrics.CreateGauge("active_actors", "Number of active actors in system");
        private readonly Gauge _alarmsUp;
        private readonly Summary _monitorLattency;

        public PrometheusMetricActor()
        {
            _alarmsUp = Metrics.CreateGauge("up_monitors", "Monitor is success if 1 or failure if 0", new GaugeConfiguration { LabelNames = _monitor_labels});
            _monitorLattency = Metrics.CreateSummary("monitor_lattency", "Monitor lattency", new SummaryConfiguration {LabelNames = _monitor_labels});

            Receive<ActiveActorMetricMessage>(_ => {
                Log.Info("Inc active actors");
                _activeActors.Inc();
            });

            Receive<InactiveActorMetricMessage>(_ => {
                Log.Info("Dec active actors");
                _activeActors.Dec();
            });

            Receive<UpMonitorMetricMessage>(m => {
                Log.Info($"Received {nameof(UpMonitorMetricMessage)} from [{m.Name}, {m.Type}, {m.Identifier}] with state {m.State}");
                if(m.State == MonitorState.Success)
                    _alarmsUp.WithLabels(m.Name, m.Type.ToString(), m.Identifier).Set(1);
                else if (m.State == MonitorState.Failed)
                    _alarmsUp.WithLabels(m.Name, m.Type.ToString(), m.Identifier).Set(0);
            });

            Receive<MonitorLattencyMessage>(m => {
                Log.Info($"Received {nameof(MonitorLattencyMessage)} from [{m.Name}, {m.Type}, {m.Identifier}] with value {m.Value}"); 
                _monitorLattency.WithLabels(m.Name, m.Type.ToString(), m.Identifier).Observe(m.Value);
            });
        }
    }
}