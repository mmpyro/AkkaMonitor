using Akka.Actor;
using Monitor.Clients;
using Monitor.Messages;
using System.Threading.Tasks;
using Monitor.Enums;
using InitParameters = Monitor.Messages.CreateDnsMonitorMessage;

namespace Monitor.Actors
{
    public class DnsMonitorActor : MonitorActor
    {
        private class AlertMessage
        {
            public AlertMessage(string content = null)
            {
                Content = content;
            }

            public string Content { get; private set; }
        }

        private class SendAlertMessage : AlertMessage
        {
            public SendAlertMessage(string content = null) : base(content) {}
        }

        private class RevokeAlertMessage : AlertMessage
        {
            public RevokeAlertMessage(string content = null) : base(content) {}
        }

        private readonly InitParameters _parameters;
        private readonly IDnsRequest _dnsRequest;

        public DnsMonitorActor(InitParameters parameters, IDnsRequest dnsRequest) : base(parameters.Name, MonitorType.DNS, parameters.Identifier, parameters.CheckInterval)
        {
            _dnsRequest = dnsRequest;
            _parameters = parameters;
        }

        protected override void Success()
        {
            base.Success();

            Receive<SendAlertMessage>(m =>
            {
                if(m.Content != null)
                {
                    var selection = Context.System.ActorSelection(ALERT_MANAGER);
                    selection.Tell(new TriggerAlertMessage(m.Content));
                    Log.Info("Sending TriggerAlertMessage");
                    Become(Failed);
                }
                else
                {
                    SendMonitorLattencyMessage();
                }
            });

            Receive<TriggerMessage>(m =>
            {
                StartMeasure();
                _dnsRequest.GetHostEntry(_parameters.Identifier)
                        .ContinueWith(t =>
                        {
                            StopMeasure();
                            if (t.IsFaulted)
                            {
                                var ex = t.Exception;
                                return new SendAlertMessage(ex.Message);
                            }
                            return new SendAlertMessage();
                        }, TaskContinuationOptions.AttachedToParent & TaskContinuationOptions.ExecuteSynchronously)
                        .PipeTo(Self);
            });
        }

        protected override void Failed()
        {
            base.Failed();
            Receive<RevokeAlertMessage>(m =>
            {
                SendMonitorLattencyMessage();
                if(m.Content != null)
                {
                    var selection = Context.System.ActorSelection(ALERT_MANAGER);
                    selection.Tell(new TriggerAlertCancelationMessage(m.Content));
                    Log.Info("Sending TriggerAlertCancelationMessage");
                    Become(Success);
                }
            });

            Receive<TriggerMessage>(m =>
            {
                StartMeasure();
                _dnsRequest.GetHostEntry(_parameters.Identifier)
                        .ContinueWith(t =>
                        {
                            StopMeasure();
                            if (t.IsCompletedSuccessfully)
                                return new RevokeAlertMessage($"DNS hostname: {_parameters.Identifier} resolved.");
                            return new RevokeAlertMessage();
                        }, TaskContinuationOptions.AttachedToParent & TaskContinuationOptions.ExecuteSynchronously)
                        .PipeTo(Self);
            });
        }

        public static Props Props(InitParameters parameters, IDnsRequest dnsRequest)
        {
            return Akka.Actor.Props.Create(() => new DnsMonitorActor(parameters, dnsRequest));
        }
    }
}