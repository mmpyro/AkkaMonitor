using Akka.Actor;
using System.Collections.Generic;
using Monitor.Messages;
using Monitor.Factories;

namespace Monitor.Actors
{
    public class MonitorManagerActor : ReceiveActor
    {
        private readonly Dictionary<int, IActorRef> _actorsRef = new Dictionary<int, IActorRef>();

        public MonitorManagerActor(IActorFactory actorFactory)
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
                foreach(var actor in _actorsRef.Values)
                {
                    actor.Tell(m);
                }
            });
        }

        public static Props Props(IActorFactory actorFactory)
        {
            return Akka.Actor.Props.Create(() => new MonitorManagerActor(actorFactory));
        }
    }
}