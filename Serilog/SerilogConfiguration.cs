using Serilog;
using Serilog.Formatting.Elasticsearch;

namespace dotnet_api_extensions.Serilog
{
    /// <summary>
    /// Configures Serilog Logger
    /// </summary>
    public static class SerilogConfiguration
    {
        private static readonly string DEFAULT_DIRECTORYPATH = "C:\\Logs";
        private static readonly string LOG_FILENAME = "application_.log";
        private static readonly long MAX_LOGFILE_SIZE = 10000000; // 10 MB
        private static readonly string DEFAULT_OUTPUT_TEMPLATE = "{Timestamp:o} [{Level:u3}] ({SourceContext}) ({UserName}) {Message}{NewLine}{Exception}";

        private const string LoggingSection = "Logging";
        private const string OutputTemplate = "OutputTemplate";

        private const string SubLoggerSection = "Subloggers";

        private const string DirectoryPath = "DirectoryPath";
        private const string FilterExpression = "FilterExpression";

        /// <summary>
        /// Configures the Serilog logger for the application
        /// </summary>
        /// <param name="hostBuilderContext"></param>
        /// <param name="logger"></param>
        public static void SetLoggerConfiguration(HostBuilderContext hostBuilderContext, LoggerConfiguration logger)
        {
            var sectionExists = hostBuilderContext.Configuration.GetChildren().Any(item => item.Key == LoggingSection);

            if (!sectionExists) return;

            var loggingSection = hostBuilderContext.Configuration.GetSection("Logging");

            logger.SetMinimumLogLevel(loggingSection);

            logger.SetLogFilter(loggingSection);

            logger.Enrich.FromLogContext();

            if (loggingSection.GetChildren().Any(item => item.Key == SubLoggerSection))
            {
                logger.SetSubLoggers(loggingSection);
            }
            else
            {
                logger.SetLogSink(loggingSection);
            }
        }

        private static void SetSubLoggers(this LoggerConfiguration logger, IConfigurationSection loggingSection)
        {
            var subloggerSection = loggingSection.GetSection(SubLoggerSection);

            //If sub-logger exists then all the loggers should be registered as sub-logger only to avoid individual filters

            logger.WriteTo.Logger((sublogger) =>
            {
                sublogger.SetLogSink(loggingSection);
                //Also exclude filters of all individual sub-loggers
                sublogger.ExcludeFilterExpressions(subloggerSection.GetChildren());
            });

            //Now configure individual sub-loggers;
            foreach (var subloggerConfig in subloggerSection.GetChildren())
            {
                logger.WriteTo.Logger((sublogger) =>
                {
                    sublogger.SetSubLogSink(loggingSection, subloggerConfig);
                    sublogger.IncludeFilterExpression(subloggerConfig);
                });
            }
        }

        /// <summary>
        /// Sets the Serilog Sink
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="configurationSection"></param>
        private static void SetLogSink(this LoggerConfiguration logger, IConfigurationSection configurationSection)
        {
            var directoryPath = configurationSection[DirectoryPath];
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                directoryPath = DEFAULT_DIRECTORYPATH;
            }
            else
            {
                directoryPath= directoryPath.GetDirectoryPath(configurationSection);
            }

            var logFilePath = Path.Combine(directoryPath, LOG_FILENAME);
            logger.SetLogSink(configurationSection, logFilePath);
        }

        private static void SetSubLogSink(this LoggerConfiguration logger, IConfigurationSection parentConfigSection, IConfigurationSection subLogConfigSection)
        {
            var directoryPath = subLogConfigSection[DirectoryPath];
            if (string.IsNullOrWhiteSpace(directoryPath)) return;

            var logFilePath = Path.Combine(directoryPath.GetDirectoryPath(parentConfigSection), LOG_FILENAME);

            logger.SetLogSink(parentConfigSection, logFilePath, subLogConfigSection);
        }

        private static void SetLogSink(this LoggerConfiguration logger, IConfigurationSection configurationSection, string logFilePath, IConfigurationSection subLogConfigSection = null)
        {
            if (!long.TryParse(configurationSection["MaxLogFileSize"], out var maxLogFileSize))
            {
                maxLogFileSize = MAX_LOGFILE_SIZE;
            }

            if (bool.TryParse(configurationSection["UseElasticSearch"], out var useElasticSearch) && useElasticSearch)
            {
                logger.SetFileSinkAsElasticSearch(logFilePath, maxLogFileSize);
            }
            else
            {
                var outputTemplateConfigSection = subLogConfigSection ?? configurationSection;
                var outputTemplate = outputTemplateConfigSection[OutputTemplate];
                if (string.IsNullOrEmpty(outputTemplate)) outputTemplate = DEFAULT_OUTPUT_TEMPLATE;

                logger.SetFileSinkAsText(logFilePath, maxLogFileSize, outputTemplate);
            }
        }

        /// <summary>
        /// Configures Serilog file sink to output logs in Elasticsearch format. Useful for using with Kibana
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="logFilePath"></param>
        /// <param name="maxLogFileSize"></param>
        private static void SetFileSinkAsElasticSearch(this LoggerConfiguration logger, string logFilePath, long maxLogFileSize)
        {
            logger.WriteTo.File(new ElasticsearchJsonFormatter(),
                path: logFilePath,
                rollingInterval: RollingInterval.Day,
                fileSizeLimitBytes: maxLogFileSize,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: null,
                shared: true
            );
        }

        /// <summary>
        /// Configures the Serilog file sink to output in Text format. Useful for Local machines.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="logFilePath"></param>
        /// <param name="maxLogFileSize"></param>
        /// <param name="outputTemplate"></param>
        private static void SetFileSinkAsText(this LoggerConfiguration logger, string logFilePath, long maxLogFileSize, string outputTemplate)
        {
            logger.WriteTo.Async(s => s.File(
                path: logFilePath,
                outputTemplate: outputTemplate,
                rollingInterval: RollingInterval.Day,
                fileSizeLimitBytes: maxLogFileSize,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: null,
                shared: true)
            );
        }

        /// <summary>
        /// Sets the minimum log level in the configuration
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="configurationSection"></param>
        private static void SetMinimumLogLevel(this LoggerConfiguration logger, IConfigurationSection configurationSection)
        {
            var minimumLevelFromConfiguration = configurationSection["MinimumLevel"];
            if (string.IsNullOrEmpty(minimumLevelFromConfiguration))
            {
                return;
            }

            if (Enum.TryParse(minimumLevelFromConfiguration, true, out LogLevel minimumLevel))
            {
                switch (minimumLevel)
                {
                    case LogLevel.Debug:
                        logger.MinimumLevel.Debug();
                        break;
                    case LogLevel.Information:
                        logger.MinimumLevel.Information();
                        break;
                    case LogLevel.Warning:
                        logger.MinimumLevel.Warning();
                        break;
                    case LogLevel.Error:
                        logger.MinimumLevel.Error();
                        break;
                    case LogLevel.Critical:
                        logger.MinimumLevel.Fatal();
                        break;
                    case LogLevel.Trace:
                        logger.MinimumLevel.Verbose();
                        break;
                    default:
                        logger.MinimumLevel.Information();
                        break;

                }
            }
        }

        /// <summary>
        /// Configures the filters to be applied on the logs before writing
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="configurationSection"></param>
        private static void SetLogFilter(this LoggerConfiguration logger, IConfigurationSection configurationSection)
        {
            if (bool.TryParse(configurationSection["OnlyAppLogs"], out var logOnlyAppLogs) && logOnlyAppLogs)
            {
                logger.Filter.ByExcluding("StartsWith(SourceContext, 'Microsoft.')");
                logger.Filter.ByExcluding("StartsWith(SourceContext, 'System.Net.Http.HttpClient.')");
                logger.Filter.ByExcluding("StartsWith(SourceContext, 'Grpc.')");
                logger.Filter.ByExcluding("StartsWith(SourceContext, 'ProtoBuf.')");
            }
        }

        private static void ExcludeFilterExpressions(this LoggerConfiguration logger, IEnumerable<IConfigurationSection> configurationSections)
        {
            foreach (var configurationSection in configurationSections)
            {
                logger.Filter.ByExcluding(configurationSection[FilterExpression]);
            }
        }

        private static void IncludeFilterExpression(this LoggerConfiguration logger, IConfigurationSection configurationSection)
        {
            logger.Filter.ByIncludingOnly(configurationSection[FilterExpression]);
        }
    }
}