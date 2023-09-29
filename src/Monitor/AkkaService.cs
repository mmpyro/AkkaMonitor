using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Bootstrap.Docker;
using Akka.Configuration;
using Akka.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MonitorLib.Actors;
using MonitorLib.Dtos;
using MonitorLib.Messages;
using Petabridge.Cmd.Cluster;
using Petabridge.Cmd.Host;
using Petabridge.Cmd.Remote;

namespace Monitor
{
    public interface IMonitorController {
        Task<IEnumerable<MonitorInfo>> ListMonitors();
        Task<MonitorStatusMessageRes> GetMonitorInfo(string name);
        void DeleteMonitor(string name);
        Task<CreateMonitorMessageRes> CreateMonitor(CreateMonitorMessageReq req);

        Task<IEnumerable<AlertInfo>> ListAlerts();
        Task<AlertDetailsMessageRes> GetAlertInfo(string name);
        void DeleteAlert(string name);
        Task<CreateAlertMessageRes> CreateAlert(CreateAlertMessageReq req);
    }


    /// <summary>
    /// <see cref="IHostedService"/> that runs and manages <see cref="ActorSystem"/> in background of application.
    /// </summary>
    public class AkkaService : IHostedService, IMonitorController
    {
        private ActorSystem ClusterSystem;
        private readonly IServiceProvider _serviceProvider;
        private IActorRef _monitorManager;
        private IActorRef _alertManager;

        public AkkaService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
             var config = ConfigurationFactory.ParseString(File.ReadAllText("app.conf")).BootstrapFromDocker();
             var bootstrap = BootstrapSetup.Create()
                .WithConfig(config) // load HOCON
                .WithActorRefProvider(ProviderSelection.Cluster.Instance); // launch Akka.Cluster

            // N.B. `WithActorRefProvider` isn't actually needed here - the HOCON file already specifies Akka.Cluster

            // enable DI support inside this ActorSystem, if needed
            var diSetup = ServiceProviderSetup.Create(_serviceProvider);

            // merge this setup (and any others) together into ActorSystemSetup
            var actorSystemSetup = bootstrap.And(diSetup);

            // start ActorSystem
            ClusterSystem = ActorSystem.Create("ClusterSys", actorSystemSetup);

            // start Petabridge.Cmd (https://cmd.petabridge.com/)
            var pbm = PetabridgeCmd.Get(ClusterSystem);
            pbm.RegisterCommandPalette(ClusterCommands.Instance);
            pbm.RegisterCommandPalette(RemoteCommands.Instance);
            pbm.Start(); // begin listening for PBM management commands

            // instantiate actors

            // use the ServiceProvider ActorSystem Extension to start DI'd actors
            var sp = ServiceProvider.For(ClusterSystem);
            _monitorManager = ClusterSystem.ActorOf(sp.Props<MonitorManagerActor>(), nameof(MonitorManagerActor)); 
            _alertManager = ClusterSystem.ActorOf(sp.Props<AlertManagerActor>(), nameof(AlertManagerActor));
            ClusterSystem.ActorOf(sp.Props<PrometheusMetricActor>(), nameof(PrometheusMetricActor));
            ClusterSystem.ActorOf(sp.Props<ConfigurationActor>(), nameof(ConfigurationActor));

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            // strictly speaking this may not be necessary - terminating the ActorSystem would also work
            // but this call guarantees that the shutdown of the cluster is graceful regardless
             await CoordinatedShutdown.Get(ClusterSystem).Run(CoordinatedShutdown.ClrExitReason.Instance);
        }

        public async Task<IEnumerable<MonitorInfo>> ListMonitors()
        {
            var res = await _monitorManager.Ask<ListMonitorMessageRes>(new ListMonitorMessageReq());
            return res.Monitors;
        }

        public async Task<MonitorStatusMessageRes> GetMonitorInfo(string name)
        {
            var res = await _monitorManager.Ask<MonitorStatusMessageRes>(new MonitorStatusMessageReq(name));
            return res;
        }

        public void DeleteMonitor(string name)
        {
            _monitorManager.Tell(new DeleteActorMessage(name.GetHashCode()));
        }

        public async Task<IEnumerable<AlertInfo>> ListAlerts()
        {
            var res = await _alertManager.Ask<ListAlertMessageRes>(new ListAlertMessageReq());
            return res.Alerts;
        }

        public async Task<AlertDetailsMessageRes> GetAlertInfo(string name)
        {
            return await _alertManager.Ask<AlertDetailsMessageRes>(new AlertDetailsMessageReq(name));
        }

        public void DeleteAlert(string name)
        {
            _alertManager.Tell(new DeleteActorMessage(name.GetHashCode()));
        }

        public async Task<CreateMonitorMessageRes> CreateMonitor(CreateMonitorMessageReq req)
        {
            return await _monitorManager.Ask<CreateMonitorMessageRes>(req);
        }

        public async Task<CreateAlertMessageRes> CreateAlert(CreateAlertMessageReq req)
        {
            return await _alertManager.Ask<CreateAlertMessageRes>(req);
        }
    }
}
