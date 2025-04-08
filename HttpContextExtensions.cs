using System.Text;

using Microsoft.Net.Http.Headers;

namespace dotnet_api_extensions
{
    /// <summary>
    /// Extensions around httpContext
    /// </summary>
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Generates response with no-cache, no-store cache control asynchronously
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="content"></param>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        public static Task GetNoCacheResponseAsync(this HttpContext httpContext, object content, int statusCode = 200)
        {
            return httpContext.Response.NoCache(statusCode).WriteAsync(content?.ToString() ?? "Empty content");
        }

        /// <summary>
        /// Generates response with no-cache, no-store cache control synchronously
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="content"></param>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        public static void GetNoCacheResponse(this HttpContext httpContext, object content, int statusCode = 200)
        {
            var bytes = Encoding.UTF8.GetBytes(content?.ToString() ?? "Empty content");

            httpContext.Response.NoCache(statusCode).Body.Write(bytes);
        }

        private static HttpResponse NoCache(this HttpResponse httpResponse, int statusCode)
        {
            httpResponse.StatusCode = statusCode;

            // Similar to: https://github.com/aspnet/Security/blob/7b6c9cf0eeb149f2142dedd55a17430e7831ea99/src/Microsoft.AspNetCore.Authentication.Cookies/CookieAuthenticationHandler.cs#L377-L379
            var headers = httpResponse.Headers;
            headers[HeaderNames.CacheControl] = "no-store, no-cache";
            headers[HeaderNames.Pragma] = "no-cache";
            headers[HeaderNames.Expires] = "Thu, 01 Jan 1970 00:00:00 GMT";

            httpResponse.ContentType = "text/plain";
            return httpResponse;
        }
    }
}