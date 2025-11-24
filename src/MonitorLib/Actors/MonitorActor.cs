using System;
using System.Diagnostics;
using Akka.Actor;
using Akka.Event;
using MonitorLib.Enums;
using MonitorLib.Messages;
using InitParamaters = MonitorLib.Messages.CreateMonitorMessageReq;

namespace MonitorLib.Actors
{
    public class MonitorActor : MetricActor, IWithTimers
    {
        private readonly Stopwatch _sw = new();
        private readonly MonitorType _monitorType;
        private readonly MonitorMode _monitorMode;
        private MonitorState _monitorState = MonitorState.Success;
        private readonly string _monitorName;
        private readonly string _identifier;
        private readonly int _checkInterval;
        public ITimerScheduler Timers { get; set; }


        public MonitorActor(InitParamaters parameters)
        {
            _monitorType = parameters.Type;
            _monitorName = parameters.Name;
            _identifier = parameters.Identifier;
            _checkInterval = parameters.Interval;
            _monitorMode = parameters.Mode;
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
            selection.Tell(new UpMonitorMetricMessage(_monitorName, _monitorType, _identifier, _monitorState));
        }

        protected void SendMonitorLattencyMessage()
        {
            var selection = Context.System.ActorSelection(PROMETHEUS);
            selection.Tell(new MonitorLattencyMessage(_monitorName, _monitorType, _identifier, _sw.Elapsed.TotalMilliseconds));
            RequestReschedule();
        }

        protected virtual void Failed()
        {
            Log.Info($"Become Failed");
            _monitorState = MonitorState.Failed;
            UpdateMonitorState();
            RequestReschedule();
            Receive<MonitorStatusMessageReq>(m =>
            {
                Sender.Tell(new MonitorStatusMessageRes(_monitorName, _checkInterval, _identifier, _monitorType, _monitorMode, MonitorState.Failed));
            });
        }

        protected virtual void Success()
        {
            Log.Info($"Become Success");
            _monitorState = MonitorState.Success;
            UpdateMonitorState();
            Receive<MonitorStatusMessageReq>(m =>
            {
                Sender.Tell(new MonitorStatusMessageRes(_monitorName, _checkInterval, _identifier, _monitorType, _monitorMode, MonitorState.Success));
            });
        }

        protected override void PreStart()
        {
            if (_monitorMode == MonitorMode.Poke)
            {
                var rnd = new Random(Guid.NewGuid().GetHashCode());
                var initialDelay = rnd.Next(0, 5);
                Timers.StartPeriodicTimer($"{_monitorType}_{_identifier}", new TriggerMessage(), TimeSpan.FromSeconds(initialDelay), TimeSpan.FromSeconds(_checkInterval));
            }
            base.PreStart();
        }

        private void RequestReschedule()
        {
            if(_monitorMode == MonitorMode.Reschedule)
            {
                Context.Parent.Tell(new IdleMonitorMessage(_monitorName.GetHashCode()));
            }
        }
    }
}