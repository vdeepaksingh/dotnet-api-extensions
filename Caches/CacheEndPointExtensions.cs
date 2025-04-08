using dotnet_api_extensions.EndPoints;

namespace dotnet_api_extensions.Caches
{
    internal static class CacheEndPointExtensions
    {
        /// <summary>
        /// Adds "clearcache" end point to bulk remove data from cachestores.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        private static IApplicationBuilder AddClearCacheStoreRepoEndPoint(this IApplicationBuilder app)
        {
            return app.AddEndPoint<CacheStoreRepoBulkRemoveMiddleware>(CacheStoreEndPointRoutes.ClearCacheRepoEndPoint);
        }

        /// <summary>
        /// Adds "cachestores" end point to list down all the configured cache stores.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        private static IApplicationBuilder AddListCacheStoreRepoEndPoint(this IApplicationBuilder app)
        {
            return app.AddEndPoint<CacheStoreRepoListStoresMiddleware>(CacheStoreEndPointRoutes.CacheStoreListEndPoint);
        }

        internal static IApplicationBuilder AddCacheStoreRepoEndPoints(this IApplicationBuilder app)
        {
            return app.AddClearCacheStoreRepoEndPoint()
                .AddListCacheStoreRepoEndPoint()
                .AddCacheStoreEndPoints();
        }

        private static IApplicationBuilder AddCacheStoreEndPoints(this IApplicationBuilder app)
        {
            return app.AddEndPoint<CacheStoreBulkRemoveMiddleware>(GetClearCacheStorePredicate());
        }

        private static Func<HttpContext, (bool Passed, string Path)> GetClearCacheStorePredicate()
        {
            return (c) =>
            {
                if (string.IsNullOrEmpty(c.Request.Path))
                    return (false, string.Empty);

                var path = c.Request.Path.Value;

                if (!path.StartsWith(CacheStoreEndPointRoutes.ClearCacheRepoEndPoint, StringComparison.OrdinalIgnoreCase)
                    && path.StartsWith(CacheStoreEndPointRoutes.ClearCacheEndPointPrefix, StringComparison.OrdinalIgnoreCase)
                    && !path.Substring(1).Contains("/")) //Should not have second slash
                {
                    //If QueryString containing "caches" key found then attach values of that to path
                    if (c.Request.Query.ContainsKey(CacheStoreEndPointQueryStrings.ClearCacheAliasNamesKey))
                    {
                        path += c.Request.Query[CacheStoreEndPointQueryStrings.ClearCacheAliasNamesKey];
                    }

                    return (true, path);
                }

                return (false, path);
            };
        }
    }
}