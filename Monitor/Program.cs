using System;
using System.IO;
using System.Reflection;
using Akka.Actor;
using Akka.Configuration;
using Monitor.Actors;
using Autofac;
using Prometheus;
using Monitor.Extensions;

namespace Monitor
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new MetricServer(hostname: "localhost", port: 8082);
            server.Start();

            using(var system = ActorSystem.Create("Monitor", LoadActorSystemConfig()))
            {
                var resolver = Container.DependencyResolver(system);
                var prometheus = system.ActorOf(resolver.Create<PrometheusMetricActor>(), nameof(PrometheusMetricActor));
                var monitorManager = system.ActorOf(resolver.Create<MonitorManagerActor>());
                var alertManager = system.ActorOf(resolver.Create<AlertManagerActor>(), nameof(AlertManagerActor));
                var configurationParser = Container.Instance.Resolve<IConfigurationParser>();
                configurationParser.Monitors.Each(m => monitorManager.Tell(m));
                configurationParser.Alerts.Each(m => alertManager.Tell(m));

                Console.WriteLine("To finish press any key..");
                Console.ReadKey();
                server.Stop();
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
