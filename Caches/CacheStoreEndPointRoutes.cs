namespace dotnet_api_extensions.Caches
{
    /// <summary>
    /// Routes specifying all the cache store end-points
    /// </summary>
    public static class CacheStoreEndPointRoutes
    {
        /// <summary>
        /// Prefix to be used for individual cache stores
        /// </summary>
        public const string ClearCacheEndPointPrefix = "/clear";

        /// <summary>
        /// End point for clear cache repo
        /// </summary>
        public const string ClearCacheRepoEndPoint = "/clearcache";

        /// <summary>
        /// End point for listing all the configured caches
        /// </summary>
        public const string CacheStoreListEndPoint = "/cachestores";
    }

    /// <summary>
    /// QueryStrings
    /// </summary>
    public static class CacheStoreEndPointQueryStrings
    {
        /// <summary>
        /// To be used with ClearCacheEndPointPrefix
        /// </summary>
        public const string ClearCacheAliasNamesKey = "caches";
    }
}
