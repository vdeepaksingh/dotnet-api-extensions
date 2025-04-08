using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace dotnet_api_extensions.ServiceCollection
{
    /// <summary>
    /// Extensions to add filters to mvc pipeline
    /// </summary>
    public static class MvcFilterExtensions
    {
        /// <summary>
        /// Add a new filter to mvc pipeline dynamically
        /// </summary>
        /// <typeparam name="TFilterType"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddFilter<TFilterType>(this IServiceCollection services) where TFilterType : IFilterMetadata
        {
            services.Configure<MvcOptions>(options => options.Filters.Add<TFilterType>());
            return services;
        }

        /// <summary>
        /// Adds a new filter at position 0 to mvc pipeline dynamically
        /// </summary>
        /// <typeparam name="TFilterType"></typeparam>
        /// <param name="services"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public static IServiceCollection AddFilter<TFilterType>(this IServiceCollection services, int order) where TFilterType : IFilterMetadata
        {
            services.Configure<MvcOptions>(options => options.Filters.Add<TFilterType>(order));
            return services;
        }
    }
}
