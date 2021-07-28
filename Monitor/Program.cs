using System;
using Akka.Actor;
using Monitor.Actors;
using Monitor.Messages;
using Monitor.Factories;
using System.Threading.Tasks;
using System.Threading;

namespace Monitor
{
    class Program
    {
        private const string SLACK_URL = "***REMOVED***";
        private const string SLACK_CHANNEL = "#akka";
        private static CancellationTokenSource _cts = new CancellationTokenSource();
        private static CancellationToken _ct = _cts.Token;

        static async Task Main(string[] args)
        {
            const string url = "http://localhost:8081";
            const int checkInterval = 5;
            using(var system = ActorSystem.Create("Monitor"))
            {
                var actorFactory = new ActorFactory(new RequestFactory(), new SlackClientFactory());
                var monitorManager = system.ActorOf(MonitorManagerActor.Props(actorFactory));
                var alertManager = system.ActorOf(AlertManagerActor.Props(actorFactory), nameof(AlertManagerActor));
                monitorManager.Tell(new CreateHttpMonitorMessage(url, 200));
                alertManager.Tell(new CreateSlackAlertMessage(SLACK_URL, SLACK_CHANNEL));

                var scheduler = Task.Run(async () => {
                    _ct.ThrowIfCancellationRequested();
                    var now = DateTime.UtcNow;
                    while(!_ct.IsCancellationRequested)
                    {
                        if((DateTime.UtcNow - now).Seconds > checkInterval)
                        {
                            monitorManager.Tell(new TriggerMessage());
                            Console.WriteLine("Sending TriggerMessage");
                            now = DateTime.UtcNow;
                        }
                        await Task.Delay(TimeSpan.FromSeconds(.5));
                    }
                }, _ct);

                Console.WriteLine("To finish press any key..");
                Console.ReadKey();
                _cts.Cancel();
                await scheduler;
            }
        }
    }
}
