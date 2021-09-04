using System;
using System.Diagnostics;
using Akka.Actor;
using Monitor.Enums;
using Monitor.Messages;

namespace Monitor.Actors
{
    public class MonitorActor : MetricActor, IWithTimers
    {
        private readonly Stopwatch _sw = new Stopwatch();
        private readonly MonitorType _monitorType;
        private MonitorState _monitorState = MonitorState.Success;
        private readonly string _monitorName;
        private readonly string _identifier;
        private readonly int _checkInterval;
        public ITimerScheduler Timers { get; set; }

        public MonitorActor(string monitorName, MonitorType monitorType, string identifier, int checkInterval)
        {
            _monitorType = monitorType;
            _monitorName = monitorName;
            _identifier = identifier;
            _checkInterval = checkInterval;
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

        protected override void PreStart()
        {
            Timers.StartPeriodicTimer($"{_monitorType}_{_identifier}", new TriggerMessage(), TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(_checkInterval));
            base.PreStart();
        }
    }
}