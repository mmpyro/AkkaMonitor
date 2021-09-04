using Akka.Actor;
using Monitor.Clients;
using Monitor.Dtos;
using Monitor.Messages;
using System.Threading.Tasks;
using Monitor.Enums;

namespace Monitor.Actors
{
    public class DnsMonitorActor : MonitorActor
    {
        private class SendAlertMessage
        {
            public string Content { get; set; }
        }

        private class RevokeAlertMessage
        {
            public string Content { get; set; }
        }

        private readonly DnsParameters _parameters;
        private readonly IDnsRequest _dnsRequest;

        public DnsMonitorActor(DnsParameters parameters, IDnsRequest dnsRequest) : base(nameof(DnsMonitorActor), MonitorType.DNS, parameters.Hostname, parameters.CheckInterval)
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
                _dnsRequest.GetHostEntry(_parameters.Hostname)
                        .ContinueWith(t =>
                        {
                            StopMeasure();
                            if (t.IsFaulted)
                            {
                                var ex = t.Exception;
                                return new SendAlertMessage { Content = ex.Message};
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
                _dnsRequest.GetHostEntry(_parameters.Hostname)
                        .ContinueWith(t =>
                        {
                            StopMeasure();
                            if (t.IsCompletedSuccessfully)
                                return new RevokeAlertMessage{ Content = $"DNS hostname: {_parameters.Hostname} resolved."};
                            return new RevokeAlertMessage();
                        }, TaskContinuationOptions.AttachedToParent & TaskContinuationOptions.ExecuteSynchronously)
                        .PipeTo(Self);
            });
        }

        public static Props Props(DnsParameters parameters, IDnsRequest dnsRequest)
        {
            return Akka.Actor.Props.Create(() => new DnsMonitorActor(parameters, dnsRequest));
        }
    }
}