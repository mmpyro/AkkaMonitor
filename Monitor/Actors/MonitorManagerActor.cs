using Akka.Actor;
using System.Collections.Generic;
using Monitor.Messages;
using Monitor.Factories;
using System;
using Monitor.Dtos;

namespace Monitor.Actors
{
    public class MonitorManagerActor : ReceiveActor
    {
        private readonly Dictionary<int, IActorRef> _actorsRef = new Dictionary<int, IActorRef>();
        private readonly IActorFactory _actorFactory;

        public MonitorManagerActor(IActorFactory actorFactory)
        {
            _actorFactory = actorFactory;

            Receive<CreateMonitorMessage>(m => {
                if(!_actorsRef.ContainsKey(m.GetHashCode()))
                {
                    var child = Context.ActorOf(CreateActor(m));
                    Context.Watch(child);
                    _actorsRef.Add(m.GetHashCode(), child);
                }
            });

            Receive<TriggerMessage>(m => {
                foreach(var item in _actorsRef)
                {
                    item.Value.Tell(m);
                }
            });
        }

        private Props CreateActor(CreateMonitorMessage message)
        {
            switch(message)
            {
                case CreateHttpMonitorMessage:
                    var m = message as CreateHttpMonitorMessage;
                    return _actorFactory.CreateHttpMonitorActor(new RequestParameters(m.Url, m.ExpectedStatusCode));
                default:
                    throw new ArgumentException($"Not supported message type: {message.GetType().Name}");
            }
        }

        public static Props Props(IActorFactory actorFactory)
        {
            return Akka.Actor.Props.Create(() => new MonitorManagerActor(actorFactory));
        }
    }
}