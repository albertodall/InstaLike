using System;
using System.Reflection;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Helpers;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;
using FluentNHibernate.Mapping;
using InstaLike.Web.Infrastructure;
using InstaLike.Web.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;
using Serilog;

namespace InstaLike.Web.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureDataAccess(this IServiceCollection services, string connectionString)
        {
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
                    m.FluentMappings
                        .Conventions.Add(
                            LazyLoad.Always(),
                            DynamicUpdate.AlwaysTrue())
                        .Conventions.Add<AssociationsMappingConvention>()
                        .Conventions.Add<NotNullGuidTypeConvention>()
                        .AddFromAssembly(Assembly.GetExecutingAssembly())
                        
                    );

            services.AddSingleton(nhConfig.BuildSessionFactory());
            services.AddScoped(sp =>
            {
                var sessionFactory = sp.GetRequiredService<ISessionFactory>();
                return sessionFactory.OpenSession();
            });

            return services;
        }

        public static IServiceCollection ConfigureAuthentication(this IServiceCollection services)
        {
            services.AddSingleton<IUserAuthenticationService, DatabaseAuthenticationService>();
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
    }

    internal class AssociationsMappingConvention : IHasManyConvention, IReferenceConvention
    {
        public void Apply(IOneToManyCollectionInstance instance)
        {
            instance.LazyLoad();
            instance.AsBag();
            instance.Cascade.AllDeleteOrphan();
            instance.Inverse();
        }

        public void Apply(IManyToOneInstance instance)
        {
            instance.LazyLoad(Laziness.Proxy);
            instance.Cascade.None();
            instance.Not.Nullable();
        }
    }

    /// <summary>
    /// Set Guid fields not nullable, for filestream references.
    /// </summary>
    internal class NotNullGuidTypeConvention : IPropertyConvention, IPropertyConventionAcceptance
    {
        public void Accept(IAcceptanceCriteria<IPropertyInspector> criteria)
        {
            criteria.Expect(x => x.Property.PropertyType == typeof(Guid));
        }

        public void Apply(IPropertyInstance instance)
        {
            instance.Not.Nullable();
        }
    }
}
