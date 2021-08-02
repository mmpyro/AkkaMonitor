using System.Diagnostics;
using Monitor.Enums;
using Monitor.Messages;

namespace Monitor.Actors
{
    public class MonitorActor : MetricActor
    {
        private readonly Stopwatch _sw = new Stopwatch();
        private MonitorType _monitorType;
        private MonitorState _monitorState = MonitorState.Success;
        private string _monitorName;
        private string _identifier;

        public MonitorActor(string monitorName, MonitorType monitorType, string identifier)
        {
            _monitorType = monitorType;
            _monitorName = monitorName;
            _identifier = identifier;
            Become(Success);
        }

        protected void StartMeasure()
        {
            _sw.Reset();
            _sw.Start();
        }

        protected void StopMeasure()
        {
            _sw.Stop();
        }

        private void UpdateMonitorState()
        {
            var selection = Context.System.ActorSelection(PROMETHEUS);
            selection.Tell(new UpMonitorMetricMessage{
                Name = _monitorName,
                State = _monitorState,
                Type = _monitorType,
                Identifier = _identifier
            });
        }

        protected void SendMonitorLattencyMessage()
        {
            var selection = Context.System.ActorSelection(PROMETHEUS);
            selection.Tell(new MonitorLattencyMessage{
                Name = _monitorName,
                Value = _sw.Elapsed.TotalMilliseconds,
                Type = _monitorType,
                Identifier = _identifier
            });
        }

        protected virtual void Failed()
        {
            Log.Info("Become Failed");
            _monitorState = MonitorState.Failed;
            UpdateMonitorState();
        }

        protected virtual void Success()
        {
            Log.Info("Become Success");
            _monitorState = MonitorState.Success;
            UpdateMonitorState();
        }
    }
}