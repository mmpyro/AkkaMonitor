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
        private static bool _wait = true;
        private static bool _sigintReceived;
        static void Main(string[] args)
        {
            Console.CancelKeyPress += (_, ea) =>
            {
                ea.Cancel = true;

                Console.WriteLine("Received SIGINT (Ctrl+C)");
                _wait = false;
                _sigintReceived = true;
            };

            AppDomain.CurrentDomain.ProcessExit += (_, _) =>
            {
                if (!_sigintReceived)
                {
                    Console.WriteLine("Received SIGTERM");
                    _wait = false;
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
                    Thread.Sleep(TimeSpan.FromSeconds(1));
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
