using Akka.Actor;
using System.Collections.Generic;
using Monitor.Messages;
using Monitor.Factories;
using Monitor.Enums;
using System.Linq;
using System;

namespace Monitor.Actors
{
    public record MonitorInfo(string Name, string Identifier, MonitorType Type, int CheckInterval);

    public class MonitorManagerActor : MetricActor
    {
        private readonly Dictionary<int, Tuple<IActorRef, MonitorInfo>> _actors = new Dictionary<int, Tuple<IActorRef, MonitorInfo>>();

        public MonitorManagerActor(IActorFactory actorFactory)
        {
            Receive<CreateMonitorMessage>(m => {
                if(!_actors.ContainsKey(m.Id))
                {
                    var child = Context.ActorOf(actorFactory.CreateMonitorActor(m));
                    Context.Watch(child);
                    _actors.Add(m.Id, Tuple.Create(child, new MonitorInfo(m.Name, m.Identifier, m.Type, m.CheckInterval)));
                }
            });

            Receive<DeleteActorMessage>(m => {
                if(_actors.TryGetValue(m.Id, out Tuple<IActorRef, MonitorInfo> actor))
                {
                    var child = actor.Item1;
                    Context.Unwatch(child);
                    _actors.Remove(m.Id);
                    child.Tell(PoisonPill.Instance);
                }
            });

            Receive<ListMonitorMessageReq>(m => {
                var response = new ListMonitorMessageRes(_actors.Values.Select(t => t.Item2).ToList().AsReadOnly());
                Sender.Tell(response);
            });

            Receive<MonitorStatusMessageReq>(m => {
                if(_actors.TryGetValue(m.Name.GetHashCode(), out Tuple<IActorRef, MonitorInfo> actor)) {
                    actor.Item1.Forward(m);
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