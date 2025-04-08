using dotnet_api_extensions.Serilog;

using Serilog;

namespace dotnet_api_extensions
{
    /// <summary>
    /// Adds components to host builder
    /// </summary>
    public static class HostBuilderExtensions
    {
        /// <summary>
        /// All defaults at one place.
        /// Adds logging and shared config file
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="findAndAddSharedConfig"></param>
        /// <returns></returns>
        public static IHostBuilder UseDefaults(this IHostBuilder builder, bool findAndAddSharedConfig)
        {
            ArgumentNullException.ThrowIfNull(builder, nameof(builder));

            // Configure the .NET Core Logger to use Serilog
            builder.UseSerilog(SerilogConfiguration.SetLoggerConfiguration);

            if (findAndAddSharedConfig)
            {
                builder.AddSharedConfigFile();
            }

            return builder;
        }
    }
}