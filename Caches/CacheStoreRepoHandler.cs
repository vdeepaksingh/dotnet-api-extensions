namespace dotnet_api_extensions.Caches
{
    internal class CacheStoreRepoHandler(ILogger<CacheStoreRepoHandler> logger, IServiceProvider serviceProvider)
    {
        private readonly ILogger _logger = logger;

        internal async Task<IDictionary<string, bool?>> ExecuteCacheStoreRepoRemoveAll()
        {
            try
            {
                var cacheStoreRepo = serviceProvider.GetService<ICacheStoreRepo>();
                if (cacheStoreRepo != null)
                {
                    return await cacheStoreRepo.BulkRemoveAll();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{summary}", "Could not call BulkRemoveAll of ICacheStoreRepo.");
            }

            return null;
        }

        internal async Task<bool?> ExecuteCacheStoreBulkRemove(string cacheStoreAliasName)
        {
            try
            {
                var cacheStoreRepo = serviceProvider.GetService<ICacheStoreRepo>();
                if (cacheStoreRepo != null)
                {
                    var cacheStore = cacheStoreRepo.GetCacheStore(cacheStoreAliasName);
                    return cacheStore != null ? await cacheStore.BulkRemove() : null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{summary} {cacheStoreAliasName}", "Could not bulk remove cache store", cacheStoreAliasName);
            }

            return null;
        }

        internal IDictionary<string, bool> ExecuteListCacheStores()
        {
            try
            {
                var cacheStoreRepo = serviceProvider.GetService<ICacheStoreRepo>();
                if (cacheStoreRepo != null)
                {
                    return cacheStoreRepo.ListCacheStores();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{summary} of ICacheStoreRepo.", "Could not call ListCacheStores");
            }

            return null;
        }
    }
}