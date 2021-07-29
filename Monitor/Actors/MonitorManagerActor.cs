using Akka.Actor;
using System.Collections.Generic;
using Monitor.Messages;
using Monitor.Factories;
using System;
using Akka.Event;

namespace Monitor.Actors
{
    public class MonitorManagerActor : ReceiveActor, IWithTimers
    {
        private readonly Dictionary<int, IActorRef> _actorsRef = new Dictionary<int, IActorRef>();
        private readonly ILoggingAdapter _log = Logging.GetLogger(Context);
        private readonly int _checkInterval;

        public MonitorManagerActor(IActorFactory actorFactory, int checkInterval)
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
                _log.Info("Sending TriggerMessage");
                foreach(var actor in _actorsRef.Values)
                {
                    actor.Tell(m);
                }
            });
            _checkInterval = checkInterval;
        }

        public ITimerScheduler Timers {get; set;}

        public static Props Props(IActorFactory actorFactory, int checkInterval)
        {
            return Akka.Actor.Props.Create(() => new MonitorManagerActor(actorFactory, checkInterval));
        }

        protected override void PreStart()
        {
            Timers.StartPeriodicTimer("check", new TriggerMessage(), TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(_checkInterval));
        }
    }
}