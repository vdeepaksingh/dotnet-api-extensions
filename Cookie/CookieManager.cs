using System.Globalization;

namespace dotnet_api_extensions.Cookie
{
    /// <summary>
    /// Code referenced from Microsoft.AspNetCore.Authentication.Cookies.ChunkingCookieManager
    /// </summary>
    internal class CookieManager
    {
        private const string ChunkKeySuffix = "C";
        private const string ChunkCountPrefix = "chunks-";

        // Parse the "chunks-XX" to determine how many chunks there should be.
        private static int ParseChunksCount(string value)
        {
            if (value != null && value.StartsWith(ChunkCountPrefix, StringComparison.Ordinal))
            {
                var chunksCountString = value.Substring(ChunkCountPrefix.Length);
                if (int.TryParse(chunksCountString, NumberStyles.None, CultureInfo.InvariantCulture, out int chunksCount))
                {
                    return chunksCount;
                }
            }
            return 0;
        }

        /// <summary>
        /// Get the reassembled cookie. Non chunked cookies are returned normally.
        /// Cookies with missing chunks just have their "chunks-XX" header returned.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="key"></param>
        /// <returns>The reassembled cookie, if any, or null.</returns>
        public static string GetRequestCookie(HttpContext context, string key)
        {
            ArgumentNullException.ThrowIfNull(context);

            ArgumentNullException.ThrowIfNull(key);

            var requestCookies = context.Request?.Cookies;

            if (requestCookies == null) return null;

            var value = requestCookies[key];
            var chunksCount = ParseChunksCount(value);
            if (chunksCount > 0)
            {
                var chunks = new string[chunksCount];
                for (var chunkId = 1; chunkId <= chunksCount; chunkId++)
                {
                    var chunk = requestCookies[key + ChunkKeySuffix + chunkId.ToString(CultureInfo.InvariantCulture)];
                    if (string.IsNullOrEmpty(chunk))
                    {
                        // Missing chunk, abort by returning the original cookie value. It may have been a false positive?
                        return value;
                    }

                    chunks[chunkId - 1] = chunk;
                }

                return string.Join(string.Empty, chunks);
            }
            return value;
        }
    }
}