using System.Reflection;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions.Helpers;
using InstaLike.Core.Domain;
using InstaLike.Web.Data;
using InstaLike.Web.Extensions;
using InstaLike.Web.Infrastructure;
using InstaLike.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;

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

            services.AddSingleton<IMessageDispatcher, MessageDispatcher>();

            var connectionString = Configuration.GetConnectionString("DefaultDatabase");
            var nhConfig = Fluently.Configure()
                .Database(
                    MsSqlConfiguration.MsSql2012.ConnectionString(connectionString)
                        .DefaultSchema("dbo")
                        .AdoNetBatchSize(20)
                        .ShowSql()
                        .FormatSql()
                        .UseReflectionOptimizer()
                )
                .Mappings(m =>
                    m.FluentMappings.AddFromAssembly(Assembly.GetExecutingAssembly())
                        .Conventions.Add(
                            LazyLoad.Always(), 
                            DynamicUpdate.AlwaysTrue()
                ));

            services.AddSingleton(nhConfig.BuildSessionFactory());
            services.AddScoped(sp =>
            {
                var sessionFactory = sp.GetRequiredService<ISessionFactory>();
                return sessionFactory.OpenSession();
            });
            services.AddScoped<IRepository<User, int>, Repository<User, int>>();
            services.RegisterCommandHandlers();

            services.AddSingleton<IUserAuthenticationService, DatabaseAuthenticationService>();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();

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
