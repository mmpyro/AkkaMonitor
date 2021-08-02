using Akka.Actor;
using Akka.Event;
using Monitor.Clients;
using Monitor.Dtos;
using Monitor.Messages;
using System.Threading.Tasks;
namespace Monitor.Actors
{
    public class HttpMonitorActor : ReceiveActor
    {
        private class SendAlertMessage
        {
            public string Content { get; set; }
        }

        private class RevokeAlertMessage
        {
            public int StatusCode { get; set; }
        }

        private const string ALERT_MANAGER = "/user/AlertManagerActor";
        private readonly ILoggingAdapter _log = Logging.GetLogger(Context);
        private readonly RequestParameters _parameters;
        private readonly IRequest _request;
        public HttpMonitorActor(RequestParameters parameters, IRequest request)
        {
            _request = request;
            _parameters = parameters;

            Become(Success);
        }

        private void Success()
        {
            _log.Info("Become Success");

            Receive<SendAlertMessage>(m => {
                var selection = Context.System.ActorSelection(ALERT_MANAGER);
                selection.Tell(new TriggerAlertMessage(m.Content));
                _log.Info("Sending TriggerAlertMessage");
                Become(Failed);
            });

            Receive<TriggerMessage>(m =>
            {
                _request.Get(_parameters.Url)
                        .ContinueWith(t => {
                            if(t.IsFaulted)
                            {
                                var ex = t.Exception;
                                return new SendAlertMessage{ Content = $"Http request has finished with error {ex.Message}"};
                            }
                            else if(t.IsCompletedSuccessfully && t.Result != _parameters.ExpectedStatusCode)
                            {
                                var content = $"Http request has finished with {t.Result} status code when call {_parameters.Url}. Expected {_parameters.ExpectedStatusCode} status code.";
                                return new SendAlertMessage{ Content = content};
                            }
                            return null;
                        }, TaskContinuationOptions.AttachedToParent & TaskContinuationOptions.ExecuteSynchronously)
                        .PipeTo(Self);
            });
        }

        private void Failed()
        {
            _log.Info("Become Failed");

            Receive<RevokeAlertMessage>(m => {
                var content = $"Http request has finished with {m.StatusCode} status code when call {_parameters.Url}.";
                var selection = Context.System.ActorSelection(ALERT_MANAGER);
                selection.Tell(new TriggerAlertCancelationMessage(content));
                _log.Info("Sending TriggerAlertCancelationMessage");
                Become(Success);
            });

            Receive<TriggerMessage>(m =>
            {
                _request.Get(_parameters.Url)
                        .ContinueWith(t => {
                                if(t.IsCompletedSuccessfully && t.Result == _parameters.ExpectedStatusCode)
                                    return new RevokeAlertMessage{StatusCode = t.Result};
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