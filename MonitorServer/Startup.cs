using System.IO;
using System.Reflection;
using Akka.Actor;
using Akka.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Monitor;
using Monitor.Actors;
using Monitor.Factories;
using Prometheus;

namespace MonitorServer
{
    public class Startup
    {
        public delegate IActorRef MonitorManagerActorProvider();
        public delegate IActorRef AlertManagerActorProvider();

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MonitorServer", Version = "v1" });
            });

            services.AddScoped<IRequestFactory, RequestFactory>();
            services.AddScoped<ISlackClientFactory, SlackClientFactory>();
            services.AddScoped<IConfigurationParser, ConfigurationParser>();
            services.AddScoped<IActorFactory, ActorFactory>();
            var sp = services.BuildServiceProvider();

            var system = ActorSystem.Create("Monitor", LoadActorSystemConfig());
            var monitorManager = system.ActorOf(MonitorManagerActor.Create(sp.GetService<IActorFactory>()), nameof(MonitorManagerActor));
            var alertManager = system.ActorOf(AlertManagerActor.Create(sp.GetService<IActorFactory>()), nameof(AlertManagerActor));
            var prometheusActor = system.ActorOf(PrometheusMetricActor.Create(), nameof(PrometheusMetricActor));
            var configurationActor = system.ActorOf(ConfigurationActor.Create(sp.GetService<IConfigurationParser>()), nameof(ConfigurationActor));
            services.AddSingleton(_ => system);
            services.AddSingleton<MonitorManagerActorProvider>(provider =>
            {
               return () => monitorManager;
            });
            services.AddSingleton<AlertManagerActorProvider>(provider =>
            {
                return () => alertManager;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MonitorServer v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapMetrics();
            });
        }

        private static Config LoadActorSystemConfig()
        {
            var executableLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var configString = File.ReadAllText(Path.Combine(executableLocation, "actorsystem.conf"));
            return ConfigurationFactory.ParseString(configString);
        }
    }
}
