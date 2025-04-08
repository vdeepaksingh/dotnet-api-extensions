using dotnet_api_extensions.Extensions;

using Microsoft.Extensions.DependencyInjection.Extensions;

namespace dotnet_api_extensions.Request
{
    /// <summary>
    /// Dependency injection for request pipeline
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Add request context accessor
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddRequestContextAccessor(this IServiceCollection services)
        {
            services.TryAddSingleton(typeof(RequestContextAccessor), (ctx) =>
             {
                 var configuration = ctx.GetRequiredService<IConfiguration>();
                 var authCookieName = configuration.GetCurrentAuthCookieName();

                 var accessor = new RequestContextAccessor
                 {
                     AuthCookieName = authCookieName
                 };

                 return accessor;
             });

            services.TryAddSingleton(typeof(IRequestContextAccessor), (serviceProvider) => serviceProvider.GetRequiredService<RequestContextAccessor>());
            return services;
        }
    }
}
