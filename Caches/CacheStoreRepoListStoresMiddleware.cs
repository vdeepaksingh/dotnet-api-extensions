using System.Text;

namespace dotnet_api_extensions.Caches
{
    /// <summary>
    /// A dynamic end-point middleware to execute "cachestores" and generate http response
    /// </summary>
    internal class CacheStoreRepoListStoresMiddleware(CacheStoreRepoHandler cacheStoreRepoHandler)
    {
        public object GetResult()
        {
            var result = cacheStoreRepoHandler.ExecuteListCacheStores();

            if (!result?.Any() ?? true) return "No configured cache stores found.";

            var resultStringBuilder = new StringBuilder();

            resultStringBuilder.AppendFormat("{0} ---- {1}{2}", "Cache Store Name", "Is Bulk Remove Enabled?", Environment.NewLine);
            resultStringBuilder.AppendLine();

            foreach (var (key, value) in result)
            {
                resultStringBuilder.AppendFormat("{0} ---- {1}{2}", key, value, Environment.NewLine);
            }

            return resultStringBuilder.ToString();
        }
    }
}