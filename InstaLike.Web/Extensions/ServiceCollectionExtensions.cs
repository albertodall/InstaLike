using System;
using System.Reflection;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions.Helpers;
using InstaLike.Web.Data;
using InstaLike.Web.Infrastructure;
using InstaLike.Web.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;
using NHibernate.Event;
using Serilog;

namespace InstaLike.Web.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureOnPremDataAccess(this IServiceCollection services, string connectionString)
        {
            var nhConfig = GetFluentConfigurationForDatabase(connectionString)
                .Mappings(m => m.FluentMappings
                    .AddFromAssembly(Assembly.GetExecutingAssembly(), t => t.IsDefined(typeof(OnPremDatabaseMappingAttribute)))
                );

            services.AddSingleton(nhConfig.BuildSessionFactory());
            services.AddScoped(sp =>
            {
                var sessionFactory = sp.GetRequiredService<ISessionFactory>();
                return sessionFactory.OpenSession();
            });

            return services;
        }

        public static IServiceCollection ConfigureAzureCloudDataAccess(this IServiceCollection services
            , string databaseConnectionString
            , string externalStorageConnectionString)
        {
            // External storage picture provider
            services.AddSingleton<IExternalStoragePictureProvider>(
                new AzureBlobStoragePictureProvider(externalStorageConnectionString)
            );

            var nhConfig = GetFluentConfigurationForDatabase(databaseConnectionString)
                .Mappings(m => m.FluentMappings
                    .AddFromAssembly(Assembly.GetExecutingAssembly(), t => t.IsDefined(typeof(CloudDatabaseMappingAttribute)))
                );

            // Attach event listeners
            services.AddSingleton(sp => 
            {
                var externalStoragePictureLoader = sp.GetRequiredService<IExternalStoragePictureProvider>();
                nhConfig.ExposeConfiguration(cfg =>
                {
                    cfg.AppendListeners(ListenerType.PreLoad  , new[] { new ExternalStorageLoadEventListener(externalStoragePictureLoader) });
                    cfg.AppendListeners(ListenerType.PreInsert, new[] { new ExternalStorageSaveEventListener(externalStoragePictureLoader) });
                    cfg.AppendListeners(ListenerType.PreUpdate, new[] { new ExternalStorageSaveEventListener(externalStoragePictureLoader) });
                });
                return nhConfig.BuildSessionFactory();
            });
            
            services.AddScoped(sp =>
            {
                var sessionFactory = sp.GetRequiredService<ISessionFactory>();
                return sessionFactory.OpenSession();
            });

            return services;
        }

        public static IServiceCollection ConfigureAuthentication(this IServiceCollection services)
        {
            services.AddScoped<IUserAuthenticationService, DatabaseAuthenticationService>();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(opt =>
                {
                    opt.Cookie.Expiration = TimeSpan.FromMinutes(30);
                    opt.Cookie.HttpOnly = true;
                    opt.LoginPath = new PathString("/Account/Login");
                });

            return services;
        }

        public static IServiceCollection ConfigureLogging(this IServiceCollection services, LoggerConfiguration config)
        {
            Log.Logger = config.CreateLogger();
            services.AddSingleton(Log.Logger);
            AppDomain.CurrentDomain.ProcessExit += (s, e) => Log.CloseAndFlush();

            return services;
        }

        public static IServiceCollection ConfigurePipeline(this IServiceCollection services)
        {
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestTimingMeter<,>));

            return services;
        }

        private static FluentConfiguration GetFluentConfigurationForDatabase(string connectionString)
        {
            return Fluently.Configure()
                .Database(
                    MsSqlConfiguration.MsSql2012.ConnectionString(connectionString)
                        .DefaultSchema("dbo")
                        .AdoNetBatchSize(20)
                        .ShowSql()
                        .FormatSql()
                        .UseReflectionOptimizer()
                )
                .Mappings(m =>
                    m.FluentMappings
                        .Conventions.Add(
                            LazyLoad.Always(),
                            DynamicUpdate.AlwaysTrue())
                        .Conventions.Add<AssociationsMappingConvention>()
                        .Conventions.Add<NotNullGuidTypeConvention>());
        }
    }
}