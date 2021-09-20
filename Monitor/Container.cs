using Akka.Actor;
using Akka.DI.AutoFac;
using Autofac;
using Microsoft.Extensions.Configuration;
using Monitor.Actors;
using Monitor.Factories;

namespace Monitor
{
    public static class Container
    {
        private static IContainer _instance = null;
        private static readonly object _locker = new object();
        public static IContainer Instance
        {
            get
            {
                if(_instance == null)
                {
                    lock(_locker)
                    {
                        if(_instance == null)
                        {
                            var configuration = new ConfigurationBuilder()
                            .AddJsonFile("Config/appsettings.json", optional: false, reloadOnChange: true)
                            .AddEnvironmentVariables()
                            .Build();
                            
                            var builder = new ContainerBuilder();
                            builder.Register((c,p) => configuration).As<IConfigurationRoot>();
                            builder.RegisterType<ConfigurationParser>().As<IConfigurationParser>();
                            builder.RegisterType<RequestFactory>().As<IRequestFactory>();
                            builder.RegisterType<SlackClientFactory>().As<ISlackClientFactory>();
                            builder.RegisterType<ActorFactory>().As<IActorFactory>();
                            builder.RegisterType<MonitorManagerActor>();
                            builder.RegisterType<AlertManagerActor>();
                            builder.RegisterType<PrometheusMetricActor>();
                            builder.RegisterType<ConfigurationActor>();
                            _instance = builder.Build();
                        }
                    }
                }
                return _instance;
            }
        }

        public static AutoFacDependencyResolver DependencyResolver(ActorSystem system)
        {
            return new AutoFacDependencyResolver(Instance, system);
        }
    }
}