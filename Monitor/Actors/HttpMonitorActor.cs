using System;
using Akka.Actor;
using Monitor.Dtos;
using Monitor.Messages;

namespace Monitor.Actors
{
    public class HttpMonitorActor : ReceiveActor
    {
        private const string ALERT_MANAGER = "/user/AlertManagerActor";
        private readonly RequestParameters _parameters;
        private readonly IRequest _request;
        public HttpMonitorActor(RequestParameters parameters, IRequest request)
        {
            _request = request;
            _parameters = parameters;

            Receive<TriggerMessage>(m =>
            {
                try
                {
                    var statusCode = _request.Get(_parameters.Url);
                    if(statusCode != _parameters.ExpectedStatusCode)
                    {
                        SendAlert($"Http request has finished with {statusCode} status code when call {_parameters.Url}. Expected {_parameters.ExpectedStatusCode} status code.");
                    }
                }
                catch(Exception ex)
                {
                    SendAlert($"Http request has finished with error {ex.Message}");
                }
            });
        }

        private void SendAlert(string content)
        {
            var selection = Context.System.ActorSelection(ALERT_MANAGER);
            selection.Tell(new TriggerAlertMessage(content));
            Console.WriteLine("Sending TriggerAlertMessage");
        }

        public static Props Props(RequestParameters parameters, IRequest request)
        {
            return Akka.Actor.Props.Create(() => new HttpMonitorActor(parameters, request));
        }
    }
}