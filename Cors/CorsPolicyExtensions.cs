using Microsoft.AspNetCore.Cors.Infrastructure;

namespace dotnet_api_extensions.Cors
{
    /// <summary>
    /// For cors extensions
    /// </summary>
    public static class CorsPolicyExtensions
    {
        /// <summary>
        /// This method configures CORS by checking "Cors:Origins" in appSettings.json.
        /// Specific origins are added by reading from "Cors:Origins".
        /// Else, all origins are allowed.
        /// 
        /// Policy name is: "__DefaultCorsPolicy"
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        internal static IServiceCollection ConfigureDefaultCors(this IServiceCollection services, IConfiguration configuration)
        {
            var corsOrigins = configuration["Cors:Origins"];

            //https://github.com/dotnet/aspnetcore/blob/master/src/Middleware/CORS/src/Infrastructure/CorsOptions.cs
            services.AddCors(o => o.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyMethod()
                       .AllowAnyHeader();

                if (!string.IsNullOrEmpty(corsOrigins))
                {
                    builder.WithSpecificOrigins(corsOrigins.Split(","));
                }
                else
                {
                    builder.AllowAnyOrigin();
                }
            }));
            return services;
        }

        /// <summary>
        /// Add allow any cors policy.
        /// Default policyName is AllowAny.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="policyName"></param>
        /// <returns></returns>
        public static IServiceCollection CorsAllowAny(this IServiceCollection services, string policyName = CorsPolicyType.AllowAny)
        {
            services.AddCors(o => o.AddPolicy(policyName, builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));
            return services;
        }

        /// <summary>
        /// Add specific origins cors policy.
        /// Origins are read from "Cors:Origins" of configuration. Origins should be comma-separated.
        /// Default policy name is SpecificOrigins. 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="policyName">Default is SpecifidOrigins. Pass a different name if you want</param>
        /// <returns></returns>
        public static IServiceCollection CorsAllowSpecificOrigins(this IServiceCollection services, IConfiguration configuration, string policyName = CorsPolicyType.SpecificOrigins)
        {
            var corsOrigins = configuration["Cors:Origins"];
            return !string.IsNullOrEmpty(corsOrigins) ? services.CorsAllowSpecificOrigins(corsOrigins.Split(","), policyName) : services;
        }

        /// <summary>
        /// Add specific origins cors policy.
        /// Default policy name is SpecificOrigins. 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="corsOrigins"></param>
        /// <param name="policyName">Default is SpecifidOrigins. Pass a different name if you want multiple policies</param>
        /// <returns></returns>
        public static IServiceCollection CorsAllowSpecificOrigins(this IServiceCollection services, string[] corsOrigins, string policyName = CorsPolicyType.SpecificOrigins)
        {
            if (corsOrigins?.Any() ?? false)
            {
                services.AddCors(o => o.AddPolicy(policyName, builder =>
                {
                    builder.WithSpecificOrigins(corsOrigins)
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                }));
            }
            return services;
        }

        private static CorsPolicyBuilder WithSpecificOrigins(this CorsPolicyBuilder builder, string[] corsOrigins)
        {
            builder.WithOrigins(corsOrigins);
            //Should allow credentials only with specific origins
            builder.AllowCredentials();
            //Allows to match wildcard domain
            //https://docs.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-3.1#set-the-allowed-origins
            builder.SetIsOriginAllowedToAllowWildcardSubdomains();
            return builder;
        }
    }
}