using Akka.Actor;
using Monitor.Dtos;
using Monitor.Factories;
using Monitor.Messages;
using System;
using System.Collections.Generic;

namespace Monitor.Actors
{
    public class AlertManagerActor : ReceiveActor
    {
        private readonly Dictionary<int, IActorRef> _actorsRef = new Dictionary<int, IActorRef>();
        private readonly IActorFactory _actorFactory;

        public AlertManagerActor(IActorFactory actorFactory)
        {
            _actorFactory = actorFactory;

            Receive<CreateAlertMessage>(m => {
                if(!_actorsRef.ContainsKey(m.GetHashCode()))
                {
                    var child = Context.ActorOf(CreateActor(m));
                    Context.Watch(child);
                    _actorsRef.Add(m.GetHashCode(), child);
                }
            });

            Receive<TriggerAlertMessage>(m => {
                foreach(var item in _actorsRef)
                {
                    item.Value.Tell(m);
                }
            });
        }

        private Props CreateActor(CreateAlertMessage message)
        {
            switch(message)
            {
                case CreateSlackAlertMessage:
                    var m = message as CreateSlackAlertMessage;
                    return _actorFactory.CreateSlackAlertActor(new SlackConfiguration(m.Url, m.Channel));
                default:
                    throw new ArgumentException($"Not supported message type: {message.GetType().Name}");
            }
        }

        public static Props Props(IActorFactory actorFactory)
        {
            return Akka.Actor.Props.Create(() => new AlertManagerActor(actorFactory));
        }
    }
}