using System;
using Akka.Actor;
using Monitor.Actors;
using Monitor.Messages;
using Monitor.Factories;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Monitor
{
    class Program
    {
        private static CancellationTokenSource _cts = new CancellationTokenSource();
        private static CancellationToken _ct = _cts.Token;

        private static IConfigurationRoot _configuration;

        static async Task Main(string[] args)
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var configurationParser = new ConfigurationParser(_configuration);

            using(var system = ActorSystem.Create("Monitor"))
            {
                var actorFactory = new ActorFactory(new RequestFactory(), new SlackClientFactory());
                var monitorManager = system.ActorOf(MonitorManagerActor.Props(actorFactory));
                var alertManager = system.ActorOf(AlertManagerActor.Props(actorFactory), nameof(AlertManagerActor));
                configurationParser.Monitors.ToList().ForEach(m => monitorManager.Tell(m));
                configurationParser.Alerts.ToList().ForEach(m => alertManager.Tell(m));

                var scheduler = Task.Run(async () => {
                    _ct.ThrowIfCancellationRequested();
                    var now = DateTime.UtcNow;
                    while(!_ct.IsCancellationRequested)
                    {
                        if((DateTime.UtcNow - now).Seconds > configurationParser.CheckInterval)
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
