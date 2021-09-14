using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AspNetCoreTestProject
{
    using BetterHostedServices;

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
            services.AddControllers();
            services.AddBetterHostedServices();
            services.AddHostedServiceAsSingleton<StubCriticalBackgroundService>();

            // Examples of how different services act. Comment one out and see for yourself!

            // services.AddHostedService<WaitingForeverHostedService>(); // This will never let the application start
            // services.AddHostedService<ImmediatelyCrashingBackgroundService>(); // This will crash the application
            // services.AddHostedService<YieldingAndThenCrashingBackgroundService>(); // This will not crash the application
            // services.AddHostedService<ImmediatelyCrashingCriticalBackgroundService>(); // Crash
            // services.AddHostedService<YieldingAndThenCrashingCriticalBackgroundService>(); // Crash

            services.AddTransient<TransientStateHolder>();
            services.AddSingleton<SingletonStateHolder>();
            services.AddScoped<ScopeStateHolder>();
            // TODO ensure that service is registered

            // services.AddPeriodicTask<PrintingPeriodicTask>(PeriodicTaskFailureMode.CRASH_APPLICATION, TimeSpan.FromSeconds(1));
            services.AddPeriodicTask<CountingPeriodicTask>(failureMode: PeriodicTaskFailureMode.CrashApplication, timeBetweenTasks: TimeSpan.FromSeconds(5));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // app.UseHttpsRedirection();

            app.UseRouting();

            // app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
