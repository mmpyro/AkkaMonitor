using System;
using System.IO;
using System.Reflection;
using Akka.Actor;
using Akka.Configuration;
using Monitor.Actors;
using Monitor.Factories;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Monitor
{
    class Program
    {
        private static IConfigurationRoot _configuration;

        static void Main(string[] args)
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
            var configurationParser = new ConfigurationParser(_configuration);

            using(var system = ActorSystem.Create("Monitor", LoadActorSystemConfig()))
            {
                var actorFactory = new ActorFactory(new RequestFactory(), new SlackClientFactory());
                var monitorManager = system.ActorOf(MonitorManagerActor.Props(actorFactory, configurationParser.CheckInterval));
                var alertManager = system.ActorOf(AlertManagerActor.Props(actorFactory), nameof(AlertManagerActor));
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
