using System;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace InstaLike.Web.Extensions
{
    internal static class LoggerConfigurationExtensions
    {
        public static void EnableFileLoggingIfConfigured(this LoggerConfiguration loggerConfig, IConfiguration configuration)
        {
            var logFileName = configuration.GetValue<string>("Logging:LogFile");
            if (!string.IsNullOrEmpty(logFileName))
            {
                var flushInterval = configuration.GetValue<int>("Logging:FlushToDiskIntervalSeconds");
                loggerConfig.WriteTo.File(
                    logFileName,
                    outputTemplate: Startup.LogEntryTemplate,
                    flushToDiskInterval: TimeSpan.FromSeconds(flushInterval));
            }
        }

        public static void EnableAppInsightsIfConfigured(this LoggerConfiguration loggerConfig, IConfiguration configuration)
        {
            var appInsightsInstumentationKey = configuration.GetValue<string>("Logging:AppInsightsInstrumentationKey");
            if (!string.IsNullOrEmpty(appInsightsInstumentationKey))
            {
                var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
                telemetryConfiguration.InstrumentationKey = appInsightsInstumentationKey;
                loggerConfig.WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces);
            }
        }
    }
}
