using Akka.Actor;
using MonitorLib.Enums;
using MonitorLib.Messages;
using Prometheus;

namespace MonitorLib.Actors
{
    public class PrometheusMetricActor : MetricActor
    {
        private readonly string[] _monitor_labels = new string []{"monitor", "type", "identifier"};
        private readonly Gauge _activeActors = Metrics.CreateGauge("monitor_active_actors", "Number of active actors in system");
        private readonly Gauge _alarmsUp;
        private readonly Summary _monitorLattency;

        public PrometheusMetricActor()
        {
            _alarmsUp = Metrics.CreateGauge("monitor_up", "Monitor is success if 1 or failure if 0", new GaugeConfiguration { LabelNames = _monitor_labels});
            _monitorLattency = Metrics.CreateSummary("monitor_lattency", "Monitor lattency", new SummaryConfiguration {LabelNames = _monitor_labels});

            Receive<ActiveActorMetricMessage>(_ => {
                _activeActors.Inc();
            });

            Receive<InactiveActorMetricMessage>(_ => {
                _activeActors.Dec();
            });

            Receive<UpMonitorMetricMessage>(m => {
                if(m.State == MonitorState.Success)
                    _alarmsUp.WithLabels(m.Labels).Set(1);
                else if (m.State == MonitorState.Failed)
                    _alarmsUp.WithLabels(m.Labels).Set(0);
            });

            Receive<MonitorLattencyMessage>(m => {
                _monitorLattency.WithLabels(m.Labels).Observe(m.Value);
            });
        }

        public static Props Create()
        {
            return Props.Create(() => new PrometheusMetricActor());
        }
    }
}