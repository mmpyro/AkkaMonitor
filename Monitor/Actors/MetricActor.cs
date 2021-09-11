using Akka.Actor;
using Akka.Event;
using Monitor.Messages;

namespace Monitor.Actors
{
    public class MetricActor : ReceiveActor
    {
        protected const string PROMETHEUS = "/user/PrometheusMetricActor";
        protected const string ALERT_MANAGER = "/user/AlertManagerActor";
        protected const string MONITOR_MANAGER = "/user/MonitorManagerActor";

        protected readonly ILoggingAdapter Log = Logging.GetLogger(Context);

        protected void SendActiveActorMetric()
        {
            var selection = Context.System.ActorSelection(PROMETHEUS);
            selection.Tell(new ActiveActorMetricMessage());
        }

        protected void SendInActiveActorMetric()
        {
            var selection = Context.System.ActorSelection(PROMETHEUS);
            selection.Tell(new InactiveActorMetricMessage());
        }

        protected override void PostStop()
        {
            SendInActiveActorMetric();
        }

        protected override void PreStart()
        {
            SendActiveActorMetric();
        }
    }
}