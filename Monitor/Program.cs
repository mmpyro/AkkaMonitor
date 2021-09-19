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
        static void Main(string[] args)
        {
            using(var server = new MetricServer(port: 8082))
            using(var system = ActorSystem.Create("Monitor", LoadActorSystemConfig()))
            {
                server.Start();
                var resolver = Container.DependencyResolver(system);
                var configuration = system.ActorOf(resolver.Create<ConfigurationActor>(), nameof(ConfigurationActor));
                var prometheus = system.ActorOf(resolver.Create<PrometheusMetricActor>(), nameof(PrometheusMetricActor));
                var monitorManager = system.ActorOf(resolver.Create<MonitorManagerActor>(), nameof(MonitorManagerActor));
                var alertManager = system.ActorOf(resolver.Create<AlertManagerActor>(), nameof(AlertManagerActor));

                Thread.Sleep(Timeout.Infinite);
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
