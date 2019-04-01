using System.Reflection;
using InstaLike.Core.Domain;
using InstaLike.Web.Extensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;

namespace InstaLike.Web
{
    public class Startup
    {
        private const string LogEntrytemplate = "[{Timestamp:HH:mm:ss} {SourceContext} {Level:u3}] - [{CorrelationID}] - {Message:lj}{NewLine}{Exception}";

        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            HostingEnvironment = hostingEnvironment;
        }

        public IConfiguration Configuration { get; }
        private IHostingEnvironment HostingEnvironment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .WriteTo.Console(outputTemplate: LogEntrytemplate)
                .WriteTo.File(
                    path: $@"D:\Temp\{Configuration["Logging:LogFile"]}",
                    outputTemplate: LogEntrytemplate); 
                    

            services.ConfigureLogging(loggerConfig);

            services.RegisterPipelineBehaviors();

            services.AddMediatR(Assembly.GetExecutingAssembly(), typeof(IEntity<>).Assembly);

            services.ConfigureDataAccess(Configuration.GetConnectionString("DefaultDatabase"));

            services.ConfigureAuthentication();

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
