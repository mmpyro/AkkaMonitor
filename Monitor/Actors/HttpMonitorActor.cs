using Akka.Actor;
using Monitor.Clients;
using Monitor.Dtos;
using Monitor.Messages;
using System.Threading.Tasks;
using Monitor.Enums;

namespace Monitor.Actors
{
    public class HttpMonitorActor : MonitorActor
    {
        private class SendAlertMessage
        {
            public string Content { get; set; }
            public int? StatusCode { get; set; }
        }

        private class RevokeAlertMessage
        {
            public string Content { get; set; }
            public int StatusCode { get; set; }
        }

        private readonly RequestParameters _parameters;
        private readonly IRequest _request;

        
        public HttpMonitorActor(RequestParameters parameters, IRequest request) : base(nameof(HttpMonitorActor), MonitorType.Http, parameters.Url)
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
                _request.Get(_parameters.Url)
                        .ContinueWith(t => {
                            StopMeasure();
                            if(t.IsFaulted)
                            {
                                var ex = t.Exception;
                                return new SendAlertMessage{ Content = $"Http request has finished with error {ex.Message}"};
                            }
                            else if(t.IsCompletedSuccessfully)
                            {
                                var content = $"Http request has finished with {t.Result} status code when call {_parameters.Url}. Expected {_parameters.ExpectedStatusCode} status code.";
                                return new SendAlertMessage{ Content = content, StatusCode = t.Result};
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
                _request.Get(_parameters.Url)
                        .ContinueWith(t => {
                                StopMeasure();
                                if(t.IsCompletedSuccessfully) {
                                    var statusCode = t.Result;
                                    var content = $"Http request has finished with {statusCode} status code when call {_parameters.Url}.";
                                    return new RevokeAlertMessage{StatusCode = statusCode, Content= content};
                                }
                                return null;
                            }, TaskContinuationOptions.AttachedToParent & TaskContinuationOptions.ExecuteSynchronously)
                        .PipeTo(Self);
            });
        }

        public static Props Props(RequestParameters parameters, IRequest request)
        {
            return Akka.Actor.Props.Create(() => new HttpMonitorActor(parameters, request));
        }
    }
}