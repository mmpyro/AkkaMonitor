using Akka.Actor;
using Monitor.Factories;
using Monitor.Messages;
using System.Collections.Generic;

namespace Monitor.Actors
{
    public class AlertManagerActor : ReceiveActor
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
        }

        public static Props Props(IActorFactory actorFactory)
        {
            return Akka.Actor.Props.Create(() => new AlertManagerActor(actorFactory));
        }
    }
}