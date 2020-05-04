using System.Reflection;
using Common.Bus;
using Common.Bus.RabbitMQ;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace Consumer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSingleton<IConnectionFactory, ConnectionFactory>();
            services.AddSingleton<IRabbitMQConnection, RabbitMQConnection>();
            services.AddSingleton<IBus, BusRabbitMQ>();
            services.AddOptions();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseMvc();
            app.UseRabbitListener();

        }
    }

    public static class ApplicationBuilderExtentions
    {
        public static IBus Bus { get; set; }

        public static IApplicationBuilder UseRabbitListener(this IApplicationBuilder app)
        {
            Bus = app.ApplicationServices.GetService<IBus>();
            var life = app.ApplicationServices.GetService<IApplicationLifetime>();
            life.ApplicationStarted.Register(OnStarted);

            life.ApplicationStopping.Register(OnStopping);
            return app;
        }

        private static void OnStarted()
        {
            Bus.Subscribe(Assembly.GetExecutingAssembly());
            Bus.SubscribeAsync(Assembly.GetExecutingAssembly());

        }

        private static void OnStopping()
        {
            Bus.Dispose();
        }

    }
}
