using System.Reflection;
using InstaLike.Core.Domain;
using InstaLike.Core.Services;
using InstaLike.Web.Extensions;
using InstaLike.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace InstaLike.Web
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            
            services.ConfigureLogging(Configuration);

            services.ConfigureTelemetry(Configuration);

            services.AddSingleton<ISequentialIdGenerator<SequentialGuid>, SequentialGuidGenerator>();

            services.ConfigurePipeline();

            services.AddMediatR(Assembly.GetExecutingAssembly(), typeof(EntityBase<>).Assembly);

            if (IsOnPremDeployment())
            {
                services.ConfigureOnPremDataAccess(Configuration.GetConnectionString("DefaultDatabase"));
            }
            else
            {
                services.ConfigureCloudDataAccess(
                    Configuration.GetConnectionString("DefaultDatabase"),
                    Configuration.GetValue<string>("ExternalStorage:AzureBlobStorage:StorageConnectionString"));
            }

            services.ConfigureAzureComputerVision(
                Configuration.GetValue<string>("ImageAnalysis:AzureComputerVision:ApiKey"),
                Configuration.GetValue<string>("ImageAnalysis:AzureComputerVision:ApiUrl"));

            services.ConfigureAuthentication();
            services.ConfigureAuthorization();

            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseRequestCorrelation();
            app.UseCookiePolicy();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }

        private bool IsOnPremDeployment()
        {
            return Configuration.GetValue<DeploymentType>("DeploymentType") == DeploymentType.OnPrem;
        }
    }
}
