using System;
using Akka.Actor;
using Akka.Event;
using Monitor.Clients;
using Monitor.Dtos;
using Monitor.Messages;
namespace Monitor.Actors
{
    public class HttpMonitorActor : ReceiveActor
    {
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
            Receive<TriggerMessage>(m =>
            {
                try
                {
                    var statusCode = _request.Get(_parameters.Url);
                    if(statusCode != _parameters.ExpectedStatusCode)
                    {
                        SendAlert($"Http request has finished with {statusCode} status code when call {_parameters.Url}. Expected {_parameters.ExpectedStatusCode} status code.");
                        Become(Failed);
                    }
                }
                catch(Exception ex)
                {
                    SendAlert($"Http request has finished with error {ex.Message}");
                    Become(Failed);
                }
            });
        }

        private void Failed()
        {
            _log.Info("Become Failed");
            Receive<TriggerMessage>(m =>
            {
                try
                {
                    var statusCode = _request.Get(_parameters.Url);
                    if(statusCode == _parameters.ExpectedStatusCode)
                    {
                        var content = $"Http request has finished with {statusCode} status code when call {_parameters.Url}.";
                        var selection = Context.System.ActorSelection(ALERT_MANAGER);
                        selection.Tell(new TriggerAlertCancelationMessage(content));
                        _log.Info("Sending TriggerAlertCancelationMessage");
                        Become(Success);
                    }
                }
                catch(Exception) {}
            });
        }

        private void SendAlert(string content)
        {
            var selection = Context.System.ActorSelection(ALERT_MANAGER);
            selection.Tell(new TriggerAlertMessage(content));
            _log.Info("Sending TriggerAlertMessage");
        }

        public static Props Props(RequestParameters parameters, IRequest request)
        {
            return Akka.Actor.Props.Create(() => new HttpMonitorActor(parameters, request));
        }
    }
}