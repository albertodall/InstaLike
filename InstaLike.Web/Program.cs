using System.Reflection;
using InstaLike.Core.Domain;
using InstaLike.Core.Services;
using InstaLike.Web.Extensions;
using InstaLike.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace InstaLike.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            builder.Host.UseSerilog(dispose: true);
            builder.Services.AddHttpContextAccessor();

            builder.Services.ConfigureLogging(builder.Configuration);
            builder.Services.ConfigureTelemetry(builder.Configuration);

            builder.Services.AddSingleton<ISequentialIdGenerator<SequentialGuid>, SequentialGuidGenerator>();

            builder.Services.ConfigurePipeline();
            builder.Services.AddMediatR(Assembly.GetExecutingAssembly(), typeof(EntityBase<>).Assembly);

            if (IsOnPremDeployment(builder.Configuration))
            {
                builder.Services.ConfigureOnPremDataAccess(builder.Configuration);
            }
            else
            {
                builder.Services.ConfigureCloudDataAccess(builder.Configuration);
            }

            builder.Services.ConfigureAzureComputerVision(builder.Configuration);

            builder.Services.ConfigureAuthentication();
            builder.Services.ConfigureAuthorization();

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            
            if (builder.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
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

            app.Run();
        }

        private static bool IsOnPremDeployment(IConfiguration configuration)
        {
            return configuration.GetValue<DeploymentType>("AppSettings:DeploymentType") == DeploymentType.OnPrem;
        }
    }
}
