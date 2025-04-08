using Microsoft.Extensions.DependencyInjection.Extensions;

namespace dotnet_api_extensions.ApiLogger
{
    /// <summary>
    /// 
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddApiLogWriter(this IServiceCollection services)
        {
            services.TryAddSingleton<IApiLogWriter, ApiLogWriter>();
            return services;
        }
    }
}
