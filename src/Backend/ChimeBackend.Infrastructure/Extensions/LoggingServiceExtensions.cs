using ChimeBackend.Application.Services;
using ChimeBackend.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace ChimeBackend.Infrastructure.Extensions;

public static class LoggingServiceExtensions
{
    public static IServiceCollection AddSerilogLogging(this IServiceCollection services, IConfiguration configuration)
    {
        var serilogConfig = configuration.GetSection("Serilog");

        var loggerConfig = new LoggerConfiguration()
            .Enrich.FromLogContext();

        if (serilogConfig.Exists())
        {
            var minimumLevel = serilogConfig["MinimumLevel"] ?? "Information";
            var level = minimumLevel.ToLower() switch
            {
                "verbose" => LogEventLevel.Verbose,
                "debug" => LogEventLevel.Debug,
                "information" => LogEventLevel.Information,
                "warning" => LogEventLevel.Warning,
                "error" => LogEventLevel.Error,
                "fatal" => LogEventLevel.Fatal,
                _ => LogEventLevel.Information
            };
            loggerConfig.MinimumLevel.Is(level);

            var overrides = serilogConfig.GetSection("Override");
            foreach (var child in overrides.GetChildren())
            {
                var overrideLevel = child.Value?.ToLower() switch
                {
                    "verbose" => LogEventLevel.Verbose,
                    "debug" => LogEventLevel.Debug,
                    "information" => LogEventLevel.Information,
                    "warning" => LogEventLevel.Warning,
                    "error" => LogEventLevel.Error,
                    "fatal" => LogEventLevel.Fatal,
                    _ => LogEventLevel.Information
                };
                loggerConfig.MinimumLevel.Override(child.Key, overrideLevel);
            }

            var consoleEnabled = serilogConfig.GetValue<bool>("Console:Enabled", true);
            var fileEnabled = serilogConfig.GetValue<bool>("File:Enabled", true);

            if (consoleEnabled)
            {
                var outputTemplate = serilogConfig["Console:OutputTemplate"]
                    ?? "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";
                loggerConfig.WriteTo.Console(outputTemplate: outputTemplate);
            }

            if (fileEnabled)
            {
                var path = serilogConfig["File:Path"] ?? "logs/api-.log";
                var rollingInterval = serilogConfig.GetValue<string>("File:RollingInterval") ?? "Day";
                var outputTemplate = serilogConfig["File:OutputTemplate"]
                    ?? "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
                var retainedFileCountLimit = serilogConfig.GetValue<int>("File:RetainedFileCountLimit", 30);

                var interval = rollingInterval.ToLower() switch
                {
                    "minute" => Serilog.RollingInterval.Minute,
                    "hour" => Serilog.RollingInterval.Hour,
                    "day" => Serilog.RollingInterval.Day,
                    "month" => Serilog.RollingInterval.Month,
                    "year" => Serilog.RollingInterval.Year,
                    _ => Serilog.RollingInterval.Day
                };

                loggerConfig.WriteTo.File(
                    path: path,
                    rollingInterval: interval,
                    outputTemplate: outputTemplate,
                    retainedFileCountLimit: retainedFileCountLimit);
            }
        }
        else
        {
            loggerConfig.MinimumLevel.Information().WriteTo.Console();
        }

        Log.Logger = loggerConfig.CreateLogger();

        services.AddSingleton<ILogService>(provider => new SerilogLogService());

        return services;
    }

}
