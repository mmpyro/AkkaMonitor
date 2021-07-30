using System;
using System.IO;
using System.Reflection;
using Akka.Actor;
using Akka.Configuration;
using Monitor.Actors;
using Monitor.Factories;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Autofac;
using Akka.DI.AutoFac;

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

            var builder = new ContainerBuilder();
            builder.RegisterType<RequestFactory>().As<IRequestFactory>();
            builder.RegisterType<SlackClientFactory>().As<ISlackClientFactory>();
            builder.RegisterType<ActorFactory>().As<IActorFactory>();
            builder.RegisterType<MonitorManagerActor>().WithParameter("checkInterval", configurationParser.CheckInterval);
            builder.RegisterType<AlertManagerActor>();
            var container = builder.Build();

            using(var system = ActorSystem.Create("Monitor", LoadActorSystemConfig()))
            {
                var resolver = new AutoFacDependencyResolver(container, system);
                var monitorManager = system.ActorOf(resolver.Create<MonitorManagerActor>());
                var alertManager = system.ActorOf(resolver.Create<AlertManagerActor>(), nameof(AlertManagerActor));
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
