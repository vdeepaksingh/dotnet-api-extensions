namespace dotnet_api_extensions.Caches
{
    public interface ICacheStoreRepo
    {
        /// <summary>
        /// Remove all the keys associated with all the cacheStores
        /// </summary>
        /// <returns></returns>
        Task<IDictionary<string, bool?>> BulkRemoveAll();

        /// <summary>
        /// Lists all the cache stores with the value of "IsBulkRemoveEnabled"
        /// </summary>
        /// <returns></returns>
        IDictionary<string, bool> ListCacheStores();

        /// <summary>
        /// Returns the cache store attached with the alias name provided
        /// </summary>
        /// <param name="aliasName"></param>
        /// <returns></returns>
        ICacheStoreBase GetCacheStore(string aliasName);
    }
}
