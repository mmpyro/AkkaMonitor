using Akka.Actor;
using Monitor.Enums;
using Monitor.Factories;
using Monitor.Messages;
using System.Collections.Generic;
using System.Linq;

namespace Monitor.Actors
{
    public record AlertInfo(string Name, AlertType Type);

    public class AlertManagerActor : MetricActor
    {
        private record AlertActorRef(IActorRef ActorRef, AlertInfo Info);
        private readonly Dictionary<int, AlertActorRef> _actors = new();

        public AlertManagerActor(IActorFactory actorFactory)
        {
            Receive<CreateAlertMessage>(m => {
                if(!_actors.ContainsKey(m.GetHashCode()))
                {
                    var child = Context.ActorOf(actorFactory.CreateAlertActor(m));
                    Context.Watch(child);
                    _actors.Add(m.GetHashCode(), new AlertActorRef(child, new AlertInfo(m.Name, m.Type)));
                }
            });

            Receive<TriggerAlertMessage>(m => {
                foreach(var actor in _actors.Values)
                {
                    actor.ActorRef.Tell(m);
                }
            });

            Receive<TriggerAlertCancelationMessage>(m => {
                foreach(var actor in _actors.Values)
                {
                    actor.ActorRef.Tell(m);
                }
            });

            Receive<DeleteActorMessage>(m => {
                if(_actors.TryGetValue(m.Id, out AlertActorRef actor))
                {
                    Context.Unwatch(actor.ActorRef);
                    _actors.Remove(m.Id);
                    actor.ActorRef.Tell(PoisonPill.Instance);
                }
            });

            Receive<ListAlertMessageReq>(m => {
                var response = new ListAlertMessageRes(_actors.Values.Select(t => t.Info).ToList().AsReadOnly());
                Sender.Tell(response);
            });

            Receive<AlertDetailsMessageReq>(m => {
                if (_actors.TryGetValue(m.Name.GetHashCode(), out AlertActorRef actor))
                {
                    actor.ActorRef.Forward(m);
                }
                else
                {
                    Sender.Tell(null);
                }
            });
        }

        public static Props Create(IActorFactory factory)
        {
            return Props.Create(() => new AlertManagerActor(factory));
        }
    }
}