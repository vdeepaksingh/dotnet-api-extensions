using dotnet_api_extensions.Authentication;
using dotnet_api_extensions.Extensions;
using dotnet_api_extensions.Request;

namespace dotnet_api_extensions.HttpClients
{
    /// <summary>
    /// Extensions for http message
    /// </summary>
    public static class HttpMessageExtensions
    {
        #region cookie

        /// <summary>
        /// Adds auth cookie by reading the same from requestContextAccessor
        /// </summary>
        /// <param name="message"></param>
        /// <param name="requestContextAccessor"></param>
        /// <returns></returns>
        public static HttpRequestMessage AddAuthCookie(this HttpRequestMessage message, IRequestContextAccessor requestContextAccessor) => 
            message.AddAuthCookie(requestContextAccessor?.GetRequestAuthCookie());

        /// <summary>
        /// Adds auth cookie by reading the same from httpContext
        /// </summary>
        /// <param name="message"></param>
        /// <param name="httpContextAccessor"></param>
        /// <returns></returns>
        public static HttpRequestMessage AddAuthCookie(this HttpRequestMessage message, IHttpContextAccessor httpContextAccessor) => 
            message.AddAuthCookie(httpContextAccessor?.HttpContext);

        /// <summary>
        /// Adds auth cookie by reading the same from httpCookieWrapper
        /// </summary>
        /// <param name="message"></param>
        /// <param name="httpCookieWrapper"></param>
        /// <returns></returns>
        public static HttpRequestMessage AddAuthCookie(this HttpRequestMessage message, HttpCookieWrapper httpCookieWrapper)
        {
            if (!string.IsNullOrEmpty(httpCookieWrapper?.Value))
            {
                message.Headers.Add("Cookie", $"{httpCookieWrapper.Name}={httpCookieWrapper.Value}");
            }

            return message;
        }

        /// <summary>
        /// Adds auth cookie by reading the same from httpContext
        /// </summary>
        /// <param name="message"></param>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static HttpRequestMessage AddAuthCookie(this HttpRequestMessage message, HttpContext httpContext) => 
            message.AddAuthCookie(httpContext?.GetAuthCookieFromRequest());

        #endregion

        #region oAuth 

        /// <summary>
        /// Adds oAuth header by reading the same from requestContextAccessor
        /// </summary>
        /// <param name="message"></param>
        /// <param name="requestContextAccessor"></param>
        /// <returns></returns>
        public static HttpRequestMessage AddOAuthToken(this HttpRequestMessage message, IRequestContextAccessor requestContextAccessor) =>
            message.AddOAuthToken(requestContextAccessor?.GetRequestOAuthToken());

        /// <summary>
        /// Adds oAuth header 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="oAuthToken"></param>
        /// <returns></returns>
        public static HttpRequestMessage AddOAuthToken(this HttpRequestMessage message, string oAuthToken)
        {
            if (!string.IsNullOrEmpty(oAuthToken))
            {
                message.Headers.Add("Authorization", oAuthToken);
            }
            return message;
        }

        /// <summary>
        /// Adds oAuth header by reading the same from httpContext
        /// </summary>
        /// <param name="message"></param>
        /// <param name="httpContextAccessor"></param>
        /// <returns></returns>
        public static HttpRequestMessage AddOAuthToken(this HttpRequestMessage message, IHttpContextAccessor httpContextAccessor) => 
            message.AddOAuthToken(httpContextAccessor?.HttpContext);

        /// <summary>
        /// Adds oAuth header by reading the same from httpContext
        /// </summary>
        /// <param name="message"></param>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static HttpRequestMessage AddOAuthToken(this HttpRequestMessage message, HttpContext httpContext) =>
            message.AddOAuthToken(httpContext?.GetOAuthTokenFromRequest());

        #endregion
    }
}