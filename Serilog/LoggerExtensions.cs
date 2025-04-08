using Serilog.Context;

namespace dotnet_api_extensions.Serilog
{
    /// <summary>
    /// Extensions over ILogger
    /// </summary>
    public static class LoggerExtensions
    {
        private const string RootDirectoryPath = "RootDirectoryPath";
        private const string RootPathTemplate = "{Root}";

        /// <summary>
        /// Wraps logger action with LogContext
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        /// <param name="loggerAction"></param>
        public static void WithContext(string propertyName, string propertyValue, Action loggerAction)
        {
            using (LogContext.PushProperty(propertyName, propertyValue))
            {
                loggerAction();
            }
        }

        /// <summary>
        /// Wraps logger action with LogContext
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="loggerAction"></param>
        public static void WithContext(Dictionary<string, string> properties, Action loggerAction)
        {
            if (!properties?.Any() ?? true) return;

            WithContextInternal(properties, 0, loggerAction);
        }

        private static void WithContextInternal(Dictionary<string, string> properties, int index, Action loggerAction)
        {
            if (properties.Count <= index) return;

            var property = properties.ElementAt(index);

            using (LogContext.PushProperty(property.Key, property.Value))
            {
                if (index == properties.Count - 1)
                {
                    loggerAction();
                }
                else
                {
                    WithContextInternal(properties, index + 1, loggerAction);
                }
            }
        }

        /// <summary>
        /// An extension method to support logging with context
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        /// <param name="logLevel"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void LogWithContext(this ILogger logger, string propertyName, string propertyValue, LogLevel logLevel, string message, params object[] args)
        {
            WithContext(propertyName, propertyValue, () => logger.Log(logLevel, message, args));
        }

        /// <summary>
        /// An extension method to support exception logging with context
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        /// <param name="logLevel"></param>
        /// <param name="exception"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void LogWithContext(this ILogger logger, string propertyName, string propertyValue, LogLevel logLevel, Exception exception, string message, params object[] args)
        {
            WithContext(propertyName, propertyValue, () => logger.Log(logLevel, exception, message, args));
        }

        /// <summary>
        /// An extension method to support logging with context
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="properties"></param>
        /// <param name="logLevel"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void LogWithContext(this ILogger logger, Dictionary<string, string> properties, LogLevel logLevel, string message, params object[] args)
        {
            WithContext(properties, () => logger.Log(logLevel, message, args));
        }

        /// <summary>
        /// An extension method to support exception logging with context
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="properties"></param>
        /// <param name="logLevel"></param>
        /// <param name="exception"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void LogWithContext(this ILogger logger, Dictionary<string, string> properties, LogLevel logLevel, Exception exception, string message, params object[] args)
        {
            WithContext(properties, () => logger.Log(logLevel, exception, message, args));
        }

        internal static string GetDirectoryPath(this string path,IConfigurationSection configurationSection)
        {
            if (!path.Contains(RootPathTemplate)) return path;

            var rootPath = configurationSection[RootDirectoryPath];

            if (string.IsNullOrWhiteSpace(rootPath))
            {
                throw new ArgumentNullException($"Logging:{RootDirectoryPath}", $"{RootDirectoryPath} is not configured");
            }

            return path.Replace(RootPathTemplate, rootPath);
        }
    }
}