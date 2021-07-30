using System;
using System.IO;
using System.Reflection;
using Akka.Actor;
using Akka.Configuration;
using Monitor.Actors;
using System.Linq;
using Autofac;

namespace Monitor
{
    class Program
    {
        static void Main(string[] args)
        {
            using(var system = ActorSystem.Create("Monitor", LoadActorSystemConfig()))
            {
                var resolver = Container.DependencyResolver(system);
                var monitorManager = system.ActorOf(resolver.Create<MonitorManagerActor>());
                var alertManager = system.ActorOf(resolver.Create<AlertManagerActor>(), nameof(AlertManagerActor));
                var configurationParser = Container.Instance.Resolve<IConfigurationParser>();
                configurationParser.Monitors.ToList().ForEach(m => monitorManager.Tell(m));
                configurationParser.Alerts.ToList().ForEach(m => alertManager.Tell(m));

                Console.WriteLine("To finish press any key..");
                Console.ReadKey();
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
