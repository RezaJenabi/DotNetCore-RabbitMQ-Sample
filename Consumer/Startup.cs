using System.Reflection;
using Consumer.RebbitMQ;
using Consumer.RebbitMQ.IRabbitMQ;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);


            services.AddSingleton(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<AutoSubscriber>>();

                var factory = new ConnectionFactory()
                {
                    HostName = Configuration["EventBusConnection"]
                };

                if (!string.IsNullOrEmpty(Configuration["EventBusUserName"]))
                {
                    factory.UserName = Configuration["EventBusUserName"];
                }

                if (!string.IsNullOrEmpty(Configuration["EventBusPassword"]))
                {
                    factory.Password = Configuration["EventBusPassword"];
                }

                return new AutoSubscriber(factory);
            });

            services.AddOptions();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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
            //Initilize Rabbit Listener in ApplicationBuilderExtentions
            app.UseRabbitListener();
            app.UseMvc();

        }
    }

    public static class ApplicationBuilderExtentions
    {
        public static AutoSubscriber Bus { get; set; }

        public static IApplicationBuilder UseRabbitListener(this IApplicationBuilder app)
        {
            Bus = app.ApplicationServices.GetService<AutoSubscriber>();
            var lifeTime = app.ApplicationServices.GetService<IApplicationLifetime>();
            var autoSubscriber = app.ApplicationServices.GetService<IAutoSubscriber>();
            
            lifeTime.ApplicationStarted.Register(() =>
            {
                autoSubscriber.Subscribe(Assembly.GetExecutingAssembly());
                autoSubscriber.SubscribeAsync(Assembly.GetExecutingAssembly());
            });

            lifeTime.ApplicationStopped.Register(callback: () => Bus.Dispose());

            return app;
        }
      
    }
}
