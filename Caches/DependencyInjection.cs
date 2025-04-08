using Microsoft.Extensions.DependencyInjection.Extensions;

namespace dotnet_api_extensions.Caches
{
    internal static class DependencyInjection
    {
        internal static IServiceCollection AddCacheDependencies(this IServiceCollection services)
        {
            //Add cacheStoreRepoHandler
            services.TryAddSingleton<CacheStoreRepoHandler>();
            //Add middlewares to handle end points
            services.TryAddTransient<CacheStoreRepoBulkRemoveMiddleware>();
            services.TryAddTransient<CacheStoreRepoListStoresMiddleware>();
            services.TryAddTransient<CacheStoreBulkRemoveMiddleware>();


            return services;
        }
    }
}