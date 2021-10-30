using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MonitorLib;
using MonitorLib.Factories;
using MonitorLib.Validators;
using Prometheus;

namespace Monitor
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MonitorServer", Version = "v1" });
            });
            services.AddTransient<IRequestFactory, RequestFactory>();
            services.AddTransient<ISlackClientFactory, SlackClientFactory>();
            services.AddTransient<IConfigurationParser, ConfigurationParser>();
            services.AddTransient<IActorFactory, ActorFactory>();
            services.AddTransient<IMonitorCreationValidator, MonitorCreationValidator>();

            // creates an instance of the ISignalRProcessor that can be handled by SignalR
            services.AddSingleton<IMonitorController, AkkaService>();

            // starts the IHostedService, which creates the ActorSystem and actors
            services.AddHostedService(sp => (AkkaService)sp.GetRequiredService<IMonitorController>());
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

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapMetrics();
            });
        }
    }
}
