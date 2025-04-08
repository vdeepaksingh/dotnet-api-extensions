namespace dotnet_api_extensions.Caches
{
    public interface ICacheStoreBase
    {
        /// <summary>
        /// Cache identifier
        /// </summary>
        string Identifier { get; }

        /// <summary>
        /// Alias name of the cache
        /// </summary>
        string AliasName { get; set; }

        /// <summary>
        /// Remove all the keys associated with this cacheStore from cache
        /// </summary>
        /// <returns></returns>
        Task<bool?> BulkRemove();
    }
}