using Akka.Actor;
using Monitor.Factories;
using Monitor.Messages;
using System.Collections.Generic;

namespace Monitor.Actors
{
    public class AlertManagerActor : MetricActor
    {
        private readonly Dictionary<int, IActorRef> _actorsRef = new Dictionary<int, IActorRef>();

        public AlertManagerActor(IActorFactory actorFactory)
        {
            Receive<CreateAlertMessage>(m => {
                if(!_actorsRef.ContainsKey(m.GetHashCode()))
                {
                    var child = Context.ActorOf(actorFactory.CreateAlertActor(m));
                    Context.Watch(child);
                    _actorsRef.Add(m.GetHashCode(), child);
                }
            });

            Receive<TriggerAlertMessage>(m => {
                foreach(var actor in _actorsRef.Values)
                {
                    actor.Tell(m);
                }
            });

            Receive<TriggerAlertCancelationMessage>(m => {
                foreach(var actor in _actorsRef.Values)
                {
                    actor.Tell(m);
                }
            });

            Receive<DeleteActorMessage>(m => {
                if(_actorsRef.TryGetValue(m.Id, out IActorRef child))
                {
                    Context.Unwatch(child);
                    _actorsRef.Remove(m.Id);
                    child.Tell(PoisonPill.Instance);
                }
            });
        }

        public static Props Create(IActorFactory factory)
        {
            return Props.Create(() => new AlertManagerActor(factory));
        }
    }
}