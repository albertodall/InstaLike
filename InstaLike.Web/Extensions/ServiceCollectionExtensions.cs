﻿using System;
using System.Reflection;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions.Helpers;
using InstaLike.Web.Data;
using InstaLike.Web.Infrastructure;
using InstaLike.Web.Security;
using InstaLike.Web.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;
using Serilog;

namespace InstaLike.Web.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureOnPremDataAccess(this IServiceCollection services, string connectionString)
        {
            var nhConfig = GetFluentConfigurationForDatabase(connectionString);

            services.AddSingleton(nhConfig.BuildSessionFactory());
            services.AddScoped(sp =>
            {
                var sessionFactory = sp.GetRequiredService<ISessionFactory>();
                return sessionFactory.OpenSession();
            });

            return services;
        }

        public static IServiceCollection ConfigureCloudDataAccess(this IServiceCollection services,
            string databaseConnectionString,
            string externalStorageConnectionString)
        {
            var nhConfig = GetFluentConfigurationForDatabase(databaseConnectionString);

            // Configure external storage
            services.AddSingleton<IHybridStorageConnectionProvider, AzureBlobStorageConnectionProvider>();
            services.AddSingleton(sp =>
            {
                var connectionProvider = sp.GetRequiredService<IHybridStorageConnectionProvider>();

                nhConfig.ExposeConfiguration(cfg => 
                {
                    cfg.SetProperty(ExternalStorageParameters.ConnectionProviderProperty, connectionProvider.GetType().FullName);
                    cfg.SetProperty(ExternalStorageParameters.ConnectionStringProperty, externalStorageConnectionString);
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

        public static IServiceCollection ConfigureAuthorization(this IServiceCollection services)
        {
            services.AddTransient<IAuthorizationHandler, PostAuthorHandler>();
            services.AddAuthorization(opt =>
            {
                opt.AddPolicy("IsPostAuthor", policy =>
                    policy.AddRequirements(new PostAuthorRequirement()));
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

        public static IServiceCollection ConfigureAzureComputerVision(this IServiceCollection services, string apiKey, string apiUrl)
        {
            services.AddSingleton<IImageRecognitionService>(new AzureComputerVisionRecognition(apiKey, apiUrl));

            return services;
        }

        private static FluentConfiguration GetFluentConfigurationForDatabase(string connectionString)
        {
            return Fluently.Configure()
                .Database(
                    MsSqlConfiguration.MsSql2012
                        .Provider<HybridStorageDriverConnectionProvider>()
                        .ConnectionString(connectionString)
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
                        .Conventions.Add<NotNullGuidTypeConvention>()
                        .AddFromAssembly(Assembly.GetExecutingAssembly()));
        }
    }
}
