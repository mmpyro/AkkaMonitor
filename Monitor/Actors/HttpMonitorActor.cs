using Akka.Actor;
using Monitor.Clients;
using Monitor.Messages;
using System.Threading.Tasks;
using Monitor.Enums;
using InitParamaters= Monitor.Messages.CreateHttpMonitorMessage;

namespace Monitor.Actors
{
    public class HttpMonitorActor : MonitorActor
    {
        private class AlertMessage
        {
            public AlertMessage(string content = null, int? statucCode =  null)
            {
                Content = content;
                StatusCode = statucCode;
            }

            public string Content { get; private set; }
            public int? StatusCode { get; private set; }
        }

        private class SendAlertMessage : AlertMessage
        {
            public SendAlertMessage(string content = null, int? statucCode =  null) : base(content, statucCode) {}

        }

        private class RevokeAlertMessage : AlertMessage
        {
            public RevokeAlertMessage(string content = null, int? statucCode =  null) : base(content, statucCode) {}
        }

        private readonly InitParamaters _parameters;
        private readonly IRequest _request;

        
        public HttpMonitorActor(InitParamaters parameters, IRequest request) : base(parameters.Name, MonitorType.Http, parameters.Identifier, parameters.CheckInterval)
        {
            _request = request;
            _parameters = parameters;
        }

        protected override void Success()
        {
            base.Success();

            Receive<SendAlertMessage>(m => {
                if(!m.StatusCode.HasValue || m.StatusCode != _parameters.ExpectedStatusCode)
                {
                    var selection = Context.System.ActorSelection(ALERT_MANAGER);
                    selection.Tell(new TriggerAlertMessage(m.Content));
                    Log.Info("Sending TriggerAlertMessage");
                    Become(Failed);
                }
                else
                    SendMonitorLattencyMessage();
            });

            Receive<TriggerMessage>(m =>
            {
                StartMeasure();
                _request.Get(_parameters.Identifier)
                        .ContinueWith(t => {
                            StopMeasure();
                            if(t.IsFaulted)
                            {
                                var ex = t.Exception;
                                return new SendAlertMessage($"Http request has finished with error {ex.Message}");
                            }
                            else if(t.IsCompletedSuccessfully)
                            {
                                var content = $"Http request has finished with {t.Result} status code when call {_parameters.Identifier}. Expected {_parameters.ExpectedStatusCode} status code.";
                                return new SendAlertMessage(content, t.Result);
                            }
                            return null;
                        }, TaskContinuationOptions.AttachedToParent & TaskContinuationOptions.ExecuteSynchronously)
                        .PipeTo(Self);
            });
        }

        protected override void Failed()
        {
            base.Failed();
            Receive<RevokeAlertMessage>(m => {
                if(m.StatusCode == _parameters.ExpectedStatusCode)
                {
                    var selection = Context.System.ActorSelection(ALERT_MANAGER);
                    selection.Tell(new TriggerAlertCancelationMessage(m.Content));
                    Log.Info("Sending TriggerAlertCancelationMessage");
                    Become(Success);
                }
                SendMonitorLattencyMessage();
            });

            Receive<TriggerMessage>(m =>
            {
                StartMeasure();
                _request.Get(_parameters.Identifier)
                        .ContinueWith(t => {
                                StopMeasure();
                                if(t.IsCompletedSuccessfully) {
                                    var statusCode = t.Result;
                                    var content = $"Http request has finished with {statusCode} status code when call {_parameters.Identifier}.";
                                    return new RevokeAlertMessage(content, statusCode);
                                }
                                return null;
                            }, TaskContinuationOptions.AttachedToParent & TaskContinuationOptions.ExecuteSynchronously)
                        .PipeTo(Self);
            });
        }

        public static Props Props(InitParamaters parameters, IRequest request)
        {
            return Akka.Actor.Props.Create(() => new HttpMonitorActor(parameters, request));
        }
    }
}