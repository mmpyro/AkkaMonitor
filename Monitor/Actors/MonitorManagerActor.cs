using Akka.Actor;
using System.Collections.Generic;
using Monitor.Messages;
using Monitor.Factories;
using System;

namespace Monitor.Actors
{
    public class MonitorManagerActor : MetricActor, IWithTimers
    {
        private readonly Dictionary<int, IActorRef> _actorsRef = new Dictionary<int, IActorRef>();
        private readonly int _checkInterval;

        public MonitorManagerActor(IActorFactory actorFactory, IConfigurationParser configuration)
        {
            Receive<CreateMonitorMessage>(m => {
                if(!_actorsRef.ContainsKey(m.GetHashCode()))
                {
                    var child = Context.ActorOf(actorFactory.CreateMonitorActor(m));
                    Context.Watch(child);
                    _actorsRef.Add(m.GetHashCode(), child);
                }
            });

            Receive<TriggerMessage>(m => {
                Log.Info("Sending TriggerMessage");
                foreach(var actor in _actorsRef.Values)
                {
                    actor.Tell(m);
                }
            });
            _checkInterval = configuration.CheckInterval;
        }

        public ITimerScheduler Timers {get; set;}

        protected override void PreStart()
        {
            Timers.StartPeriodicTimer("check", new TriggerMessage(), TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(_checkInterval));
            base.PreStart();
        }
    }
}