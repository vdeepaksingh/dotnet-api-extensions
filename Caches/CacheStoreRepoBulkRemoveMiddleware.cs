using System.Text;

using dotnet_api_extensions.EndPoints;

namespace dotnet_api_extensions.Caches
{
    /// <summary>
    /// A dynamic end-point middleware to execute "clearcache" and generate http response
    /// </summary>
    internal class CacheStoreRepoBulkRemoveMiddleware(CacheStoreRepoHandler cacheStoreRepoHandler) : IEndPointMiddleware
    {
        public async Task<object> GetResultAsync()
        {
            var result = await cacheStoreRepoHandler.ExecuteCacheStoreRepoRemoveAll();

            if (!result?.Any() ?? true) return "Failed to clean as no cache instances found";

            var resultStringBuilder = new StringBuilder();

            if(result is not null)
            {
                foreach (var (key, value) in result)
                {
                    resultStringBuilder.AppendFormat("{0} - {1}{2}", key, !value.HasValue ? "Nothing to clean" : value.Value.ToString(), Environment.NewLine);
                }
            }

            return resultStringBuilder.ToString();
        }
    }
}