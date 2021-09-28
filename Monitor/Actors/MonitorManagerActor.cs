using Akka.Actor;
using System.Collections.Generic;
using Monitor.Messages;
using Monitor.Factories;
using Monitor.Enums;
using System.Linq;

namespace Monitor.Actors
{
    public record MonitorInfo(string Name, string Identifier, MonitorType Type, int CheckInterval);

    public class MonitorManagerActor : MetricActor
    {
        private record MonitorActorRef(IActorRef ActorRef, MonitorInfo Info);
        private readonly Dictionary<int, MonitorActorRef> _actors = new();

        public MonitorManagerActor(IActorFactory actorFactory)
        {
            Receive<CreateMonitorMessage>(m => {
                if(!_actors.ContainsKey(m.Id))
                {
                    var child = Context.ActorOf(actorFactory.CreateMonitorActor(m));
                    Context.Watch(child);
                    _actors.Add(m.Id, new MonitorActorRef(child, new MonitorInfo(m.Name, m.Identifier, m.Type, m.CheckInterval)));
                }
            });

            Receive<DeleteActorMessage>(m => {
                if(_actors.TryGetValue(m.Id, out MonitorActorRef actor))
                {
                    var child = actor.ActorRef;
                    Context.Unwatch(child);
                    _actors.Remove(m.Id);
                    child.Tell(PoisonPill.Instance);
                }
            });

            Receive<ListMonitorMessageReq>(m => {
                var response = new ListMonitorMessageRes(_actors.Values.Select(t => t.Info).ToList().AsReadOnly());
                Sender.Tell(response);
            });

            Receive<MonitorStatusMessageReq>(m => {
                if(_actors.TryGetValue(m.Name.GetHashCode(), out MonitorActorRef actor)) {
                    actor.ActorRef.Forward(m);
                } else {
                    Sender.Tell(null);
                }
            });
        }

        public static Props Create(IActorFactory actorFactory)
        {
            return Props.Create(() => new MonitorManagerActor(actorFactory));
        }
    }
}