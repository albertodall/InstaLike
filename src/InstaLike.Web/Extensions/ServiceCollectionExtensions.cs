using System;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Reflection;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions.Helpers;
using InstaLike.Web.Data;
using InstaLike.Web.Infrastructure;
using InstaLike.Web.Security;
using InstaLike.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;
using NHibernate.Driver;
using OpenAI;
using OpenAI.Chat;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace InstaLike.Web.Extensions;

internal static class ServiceCollectionExtensions
{
    private const string AzureOpenAIScope = "https://cognitiveservices.azure.com/.default";

    public static IServiceCollection ConfigureDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseConnectionString = configuration.GetConnectionString("InstalikeDatabase")
            ?? throw new InvalidOperationException("Connection string 'InstalikeDatabase' has not been configured.");
        var storageSettings = AzureBlobStorageSettings.FromConfiguration(configuration);

        var nhConfig = GetFluentConfigurationForDatabase(databaseConnectionString)
            .ExposeConfiguration(cfg =>
            {
                cfg.SetProperty(ExternalStorageParameters.ConnectionProviderProperty, typeof(AzureBlobStorageConnectionProvider).AssemblyQualifiedName);
                cfg.SetProperty(ExternalStorageParameters.ConnectionStringProperty, storageSettings.ConnectionString ?? string.Empty);
                cfg.SetProperty(ExternalStorageParameters.ServiceUriProperty, storageSettings.ServiceUri ?? string.Empty);
            });

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
        services.AddScoped<IUserAuthenticationService, DatabaseAuthenticationService>();
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(opt =>
            {
                opt.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                opt.Cookie.HttpOnly = true;
                opt.LoginPath = new PathString("/Account/Login");
            });

        return services;
    }

    public static IServiceCollection ConfigureAuthorization(this IServiceCollection services)
    {
        services.AddTransient<IAuthorizationHandler, PostAuthorRequirementHandler>();
        services.AddAuthorization(opt =>
        {
            opt.AddPolicy("IsPostAuthor", policy =>
                policy.AddRequirements(new PostAuthorRequirement()));
        });

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
            services.AddSingleton<IImageRecognitionService, DisabledImageRecognition>();
            return services;
        }

        // Any OpenAI-compatible API: the Azure OpenAI v1 endpoint (where the model is the deployment name) or a local Ollama server.
        // Without an API key, Azure OpenAI is called with the application's Azure identity.
        var options = new OpenAIClientOptions { Endpoint = new Uri(endpoint) };
        var chatClient = string.IsNullOrEmpty(apiKey)
            ? CreateEntraIdChatClient(model, options)
            : new ChatClient(model, new ApiKeyCredential(apiKey), options);

        services.AddChatClient(chatClient.AsIChatClient());
        services.AddSingleton<IImageRecognitionService, ChatClientImageTagging>();

        return services;
    }

#pragma warning disable OPENAI001 // Token authentication is the documented way to call the Azure OpenAI v1 endpoint with Entra ID.
    private static ChatClient CreateEntraIdChatClient(string model, OpenAIClientOptions options) =>
        new(model, new BearerTokenPolicy(AzureIdentity.Credential, AzureOpenAIScope), options);
#pragma warning restore OPENAI001

    private static FluentConfiguration GetFluentConfigurationForDatabase(string connectionString)
    {
        return Fluently.Configure()
            .Database(
                MsSqlConfiguration.MsSql2012
                    .Driver<MicrosoftDataSqlClientDriver>()
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
