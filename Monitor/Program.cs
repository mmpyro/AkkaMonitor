using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Akka.Actor;
using Akka.Configuration;
using Monitor.Actors;
using Prometheus;

namespace Monitor
{
    class Program
    {
        private static bool _sigintReceived = false;
        private static bool _wait = true;
        static void Main(string[] args)
        {
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                _wait = false;
                _sigintReceived = true;
                Console.WriteLine("Closing application received SIGINT.");
            };

            AppDomain.CurrentDomain.ProcessExit += (_, _) =>
            {
                if (!_sigintReceived)
                {
                    _wait = false;
                    Console.WriteLine("Closing application received SIGTERM.");
                }
            };

            using (var server = new MetricServer(port: 8082))
            using(var system = ActorSystem.Create("Monitor", LoadActorSystemConfig()))
            {
                server.Start();
                var resolver = Container.DependencyResolver(system);
                var configuration = system.ActorOf(resolver.Create<ConfigurationActor>(), nameof(ConfigurationActor));
                var prometheus = system.ActorOf(resolver.Create<PrometheusMetricActor>(), nameof(PrometheusMetricActor));
                var monitorManager = system.ActorOf(resolver.Create<MonitorManagerActor>(), nameof(MonitorManagerActor));
                var alertManager = system.ActorOf(resolver.Create<AlertManagerActor>(), nameof(AlertManagerActor));

                while(_wait)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(.5));
                }
            }
        }

        private static Config LoadActorSystemConfig()
        {
            var executableLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var configString = File.ReadAllText(Path.Combine(executableLocation, "actorsystem.conf"));
            return ConfigurationFactory.ParseString(configString);
        }
    }
}
