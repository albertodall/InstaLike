using System;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Reflection;
using Azure.Storage.Blobs;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions.Helpers;
using InstaLike.Web.Accounts;
using InstaLike.Web.Data;
using InstaLike.Web.Media;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NHibernate;
using NHibernate.Driver;
using OpenAI;
using OpenAI.Chat;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace InstaLike.Web.Infrastructure
{
    internal static class ServiceCollectionExtensions
    {
        private const string AzureOpenAIScope = "https://cognitiveservices.azure.com/.default";

        public static IServiceCollection ConfigureDataAccess(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            var databaseConnectionString = configuration.GetConnectionString("InstalikeDatabase")
                ?? throw new InvalidOperationException("Connection string 'InstalikeDatabase' has not been configured.");
            var nhConfig = GetFluentConfigurationForDatabase(databaseConnectionString);
            if (environment.IsDevelopment())
            {
                nhConfig.ExposeConfiguration(cfg => cfg
                    .SetProperty(NHibernate.Cfg.Environment.ShowSql, "true")
                    .SetProperty(NHibernate.Cfg.Environment.FormatSql, "true"));
            }

            services.AddSingleton(nhConfig.BuildSessionFactory());
            services.AddScoped(sp =>
            {
                var sessionFactory = sp.GetRequiredService<ISessionFactory>();
                return sessionFactory.OpenSession();
            });
            services.AddScoped(sp =>
            {
                var sessionFactory = sp.GetRequiredService<ISessionFactory>();
                return sessionFactory.OpenStatelessSession();
            });

            return services;
        }

        public static IServiceCollection ConfigureMediator(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg
                .RegisterServicesFromAssembly(Assembly.GetExecutingAssembly())
                .AddOpenBehavior(typeof(UnitOfWorkBehavior<,>)));
            services.AddScoped<UnitOfWork>();

            return services;
        }

        public static IServiceCollection ConfigurePictureStorage(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetValue<string>("ExternalStorage:AzureBlobStorage:ConnectionString");
            var serviceUri = configuration.GetValue<string>("ExternalStorage:AzureBlobStorage:ServiceUri");

            // A connection string for Azurite;
            // in Azure, the service URI with the application's Azure identity.
            var blobServiceClient = string.IsNullOrEmpty(connectionString)
                ? new BlobServiceClient(new Uri(serviceUri!), AzureIdentity.Credential)
                : new BlobServiceClient(connectionString);

            services.AddSingleton(blobServiceClient);
            services.AddSingleton<IPictureStore, AzureBlobPictureStore>();
            services.AddHostedService<UnusedPictureCleanupService>();

            return services;
        }

        public static IServiceCollection ConfigureAuthentication(this IServiceCollection services)
        {
            services.AddScoped<SecurityStampValidator>();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(opt =>
                {
                    opt.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                    opt.SlidingExpiration = true;
                    opt.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    opt.LoginPath = new PathString("/Account/Login");
                    opt.AccessDeniedPath = new PathString("/Account/AccessDenied");
                    opt.EventsType = typeof(SecurityStampValidator);
                });

            return services;
        }

        public static IServiceCollection ConfigureAuthorization(this IServiceCollection services)
        {
            services.AddAuthorizationBuilder()
                .SetFallbackPolicy(
                    new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build()
                );

            return services;
        }

        public static IServiceCollection ConfigureTelemetry(this IServiceCollection services, IConfiguration configuration)
        {
            if (!string.IsNullOrEmpty(configuration.GetValue<string>("ApplicationInsights:ConnectionString")))
            {
                services.AddApplicationInsightsTelemetry(options => options.Credential = AzureIdentity.Credential);
            }

            return services;
        }

        public static IServiceCollection ConfigureOpenTelemetry(this IServiceCollection services, IConfiguration configuration)
        {
            var openTelemetry = services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService("InstaLike.Web"))
                .WithTracing(tracing => tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation())
                .WithMetrics(metrics => metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddMeter("System.Runtime"))
                .WithLogging();

            services.Configure<OpenTelemetryLoggerOptions>(options => options.IncludeFormattedMessage = true);

            if (!string.IsNullOrEmpty(configuration.GetValue<string>("OTEL_EXPORTER_OTLP_ENDPOINT")))
            {
                openTelemetry.UseOtlpExporter();
            }

            return services;
        }

        public static IServiceCollection ConfigureImageTagging(this IServiceCollection services, IConfiguration configuration)
        {
            var endpoint = configuration.GetValue<string>("ImageAnalysis:Endpoint");
            var apiKey = configuration.GetValue<string>("ImageAnalysis:ApiKey");
            var model = configuration.GetValue<string>("ImageAnalysis:Model");

            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(model))
            {
                throw new InvalidOperationException("Image tagging has not been configured. Set 'ImageAnalysis:Endpoint' and 'ImageAnalysis:Model'.");
            }

            // Any OpenAI-compatible API: the Azure OpenAI v1 endpoint (where the model is the deployment name) or a local Ollama server.
            // Without an API key, Azure OpenAI is called with the application's Azure identity.
            var options = new OpenAIClientOptions { Endpoint = new Uri(endpoint) };
#pragma warning disable OPENAI001 // Token authentication is the documented way to call the Azure OpenAI v1 endpoint with Entra ID.
            var chatClient = string.IsNullOrEmpty(apiKey)
                ? new ChatClient(model, new BearerTokenPolicy(AzureIdentity.Credential, AzureOpenAIScope), options)
                : new ChatClient(model, new ApiKeyCredential(apiKey), options);
#pragma warning restore OPENAI001

            services.AddChatClient(chatClient.AsIChatClient());
            services.AddSingleton<IImageRecognitionService, ChatClientImageTagging>();

            return services;
        }

        internal static FluentConfiguration GetFluentConfigurationForDatabase(string connectionString)
        {
            return Fluently.Configure()
                .Database(MsSqlConfiguration.MsSql2012
                    .Driver<MicrosoftDataSqlClientDriver>()
                    .ConnectionString(connectionString)
                    .DefaultSchema("dbo")
                    .AdoNetBatchSize(20)
                    .UseReflectionOptimizer())
                .Mappings(m =>
                    m.FluentMappings
                        .Conventions.Add(
                            LazyLoad.Always(),
                            DynamicInsert.AlwaysTrue(),
                            DynamicUpdate.AlwaysTrue())
                        .Conventions.Add<AssociationsMappingConvention>()
                        .AddFromAssembly(Assembly.GetExecutingAssembly()));
        }
    }
}
