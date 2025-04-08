using System.Text;
using dotnet_api_extensions.EndPoints;

namespace dotnet_api_extensions.Caches
{
    /// <summary>
    /// A dynamic end-point middleware to execute "clear{cacheStoreAliasName}" and generate http response
    /// </summary>
    internal class CacheStoreBulkRemoveMiddleware(ILogger<CacheStoreBulkRemoveMiddleware> logger,
        CacheStoreRepoHandler cacheStoreRepoHandler) : IEndPointMiddlewareInternal
    {
        private readonly ILogger _logger = logger;
        private string _cacheStoreAliasName;

        public async Task<object> GetResultAsync()
        {
            if (_cacheStoreAliasName.Contains(","))
            {
                return await HandleMultiple();
            }

            var result = await HandleSingle(_cacheStoreAliasName);
            return result.HasValue ? result.Value.ToString() : "Nothing to clean OR Required cache store not found.";
        }

        private Task<bool?> HandleSingle(string cacheStoreAliasName) => cacheStoreRepoHandler.ExecuteCacheStoreBulkRemove(cacheStoreAliasName);

        private async Task<object> HandleMultiple()
        {
            var cacheNames = _cacheStoreAliasName.Split(",");

            var resultStringBuilder = new StringBuilder();

            foreach (var cacheName in cacheNames)
            {
                var result = await HandleSingle(cacheName);
                resultStringBuilder.AppendFormat("{0} - {1}{2}", cacheName, !result.HasValue ? "Nothing to clean" : result.Value.ToString(), Environment.NewLine);
            }

            return resultStringBuilder.ToString();
        }

        public void SetPath(string path)
        {
            if (string.IsNullOrEmpty(path) || !path.StartsWith(CacheStoreEndPointRoutes.ClearCacheEndPointPrefix))
            {
                throw new Exception("Invalid path set for cache store bulk remove middleware.");
            }
            _cacheStoreAliasName = path.Substring(CacheStoreEndPointRoutes.ClearCacheEndPointPrefix.Length);
            _logger.LogDebug("Alias name used for path {path} is {aliasName}", path, _cacheStoreAliasName);
        }
    }
}