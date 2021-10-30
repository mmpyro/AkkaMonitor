using Akka.Actor;
using System.Collections.Generic;
using MonitorLib.Messages;
using MonitorLib.Factories;
using MonitorLib.Enums;
using System.Linq;
using System;
using MonitorLib.Dtos;

namespace MonitorLib.Actors
{
    public class MonitorManagerActor : MetricActor, IWithTimers
    {
        private class MonitorActorRef
        {
            public MonitorActorRef(IActorRef ActorRef, MonitorInfo Info)
            {
                this.ActorRef = ActorRef;
                this.Info = Info;
            }

            public IActorRef ActorRef { get; set; }
            public MonitorInfo Info { get; }
        }
        private readonly Dictionary<int, MonitorActorRef> _actors = new();

        public ITimerScheduler Timers { get; set; }

        public MonitorManagerActor(IActorFactory actorFactory)
        {
            Receive<CreateMonitorMessageReq>(m => {
                if (!_actors.ContainsKey(m.Id) && m.Mode == MonitorMode.Poke)
                {
                    var child = Context.ActorOf(actorFactory.CreateMonitorActor(m));
                    Context.Watch(child);
                    _actors.Add(m.Id, new MonitorActorRef(child, new MonitorInfo(m.Name, m.Identifier, m.Type, m.Interval, m.Mode)));
                    Sender.Tell(new CreateMonitorMessageRes(m.Name));
                }
                else if (!Timers.IsTimerActive(m.Id) && !_actors.ContainsKey(m.Id) && m.Mode == MonitorMode.Reschedule)
                {
                    _actors.Add(m.Id, new MonitorActorRef(null, new MonitorInfo(m.Name, m.Identifier, m.Type, m.Interval, m.Mode)));
                    Timers.StartPeriodicTimer(m.Id, new ScheduleMonitorMessage(m), TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(m.Interval));
                    Sender.Tell(new CreateMonitorMessageRes(m.Name));
                }
            });

            Receive<DeleteActorMessage>(m => {
                if (_actors.TryGetValue(m.Id, out MonitorActorRef actor))
                {
                    var child = actor?.ActorRef;
                    _actors.Remove(m.Id);
                    if (Timers.IsTimerActive(m.Id))
                    {
                        Timers.Cancel(m.Id);
                    }
                    if (child != null)
                    {
                        Context.Unwatch(child);
                        child.Tell(PoisonPill.Instance);
                    }
                }
            });

            Receive<ListMonitorMessageReq>(m => {
                var response = new ListMonitorMessageRes(_actors.Values.Select(t => t.Info).ToList().AsReadOnly());
                Sender.Tell(response);
            });

            Receive<MonitorStatusMessageReq>(m => {
                if (_actors.TryGetValue(m.Name.GetHashCode(), out MonitorActorRef actor) && actor?.ActorRef != null)
                {
                    actor.ActorRef.Forward(m);
                }
                else
                {
                    Sender.Tell(null);
                }
            });

            Receive<ScheduleMonitorMessage>(msg =>
            {
                var m = msg.MonitorMessageReq;
                var child = Context.ActorOf(actorFactory.CreateMonitorActor(m));
                Context.Watch(child);
                _actors[m.Id].ActorRef = child;
                child.Tell(new TriggerMessage());
            });

           Receive<IdleMonitorMessage>(m =>
           {
               if (_actors.TryGetValue(m.Id, out MonitorActorRef actor))
               {
                   var child = actor.ActorRef;
                   Context.Unwatch(child);
                   _actors[m.Id].ActorRef = null;
                   child.Tell(PoisonPill.Instance);
               }
           });
        }

        public static Props Create(IActorFactory actorFactory)
        {
            return Props.Create(() => new MonitorManagerActor(actorFactory));
        }
    }
}