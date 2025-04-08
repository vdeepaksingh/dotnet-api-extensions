using dotnet_api_extensions.DataProtection;

using Microsoft.AspNetCore.DataProtection;

namespace dotnet_api_extensions.DataProtection
{
    /// <summary>
    /// Extensions for data protection
    /// </summary>
    public static class DataProtectionExtensions
    {
        /// <summary>
        /// Section name for data protection
        /// </summary>
        public const string DataProtectionSection = "DataProtection";

        /// <summary>
        /// 
        /// </summary>
        public const string KeyFolderName = "KeyFolder";

        /// <summary>
        /// 
        /// </summary>
        public const string AppName = "AppName";

        private static readonly string DefaultAppName = "DefaultApp";

        internal static IServiceCollection AddDataProtection(this IServiceCollection services, IConfiguration configuration)
        {
            var sectionExists = configuration.GetChildren().Any(item => item.Key == DataProtectionSection);

            if (!sectionExists)
            {
                services.AddDataProtection();
            }
            else
            {
                var dataProtectionSection = configuration.GetSection(DataProtectionSection);

                if (dataProtectionSection is null || dataProtectionSection[KeyFolderName] is null) return services;

                var appName = dataProtectionSection[AppName];

                if (string.IsNullOrEmpty(appName)) appName = DefaultAppName;

                services.AddDataProtection((options) => options.ApplicationDiscriminator = appName)
                    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionSection[KeyFolderName]!));
            }

            return services;
        }
    }
}