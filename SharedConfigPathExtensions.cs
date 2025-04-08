namespace dotnet_api_extensions
{
    /// <summary>
    /// To handle shared config paths
    /// </summary>
    public static class SharedConfigPathExtensions
    {
        /// <summary>
        /// Default path to be used. {0} will be replaced with environment name
        /// </summary>
        public const string DEFAULT_SHAREDCONFIG_PATH = @"D:\IIS\MicroServices\Configuration\{0}\SharedSettings.json";

        /// <summary>
        /// A section in appsettings to mention shared config path. This path should have file name too.
        /// </summary>
        public const string SharedConfigPath = "SharedConfigPath";

        internal static IHostBuilder AddSharedConfigFile(this IHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((hostingContext, config) =>
            {
                var env = hostingContext.HostingEnvironment;

                var fileLocation = hostingContext.Configuration[SharedConfigPath];

                if (string.IsNullOrEmpty(fileLocation))
                {
                    fileLocation = string.Format(DEFAULT_SHAREDCONFIG_PATH, env.EnvironmentName);
                }

                if (File.Exists(fileLocation))
                {
                    //Load shared settings to override changes to appsettings.json
                    config.AddJsonFile(fileLocation, optional: true, reloadOnChange: true);
                }
            });

            return builder;
        }
    }
}
