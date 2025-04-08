using dotnet_api_extensions.Authentication;
using dotnet_api_extensions.User;

namespace dotnet_api_extensions.Request
{
    /// <summary>
    /// Stores application specific request context
    /// </summary>
    public interface IRequestContextAccessor
    {
        /// <summary>
        /// Fetches current logged in user
        /// </summary>
        /// <returns></returns>
        UserInfo GetLoggedInUser();

        /// <summary>
        /// Fetches current request authentication cookie
        /// </summary>
        /// <returns></returns>
        HttpCookieWrapper GetRequestAuthCookie();

        /// <summary>
        /// Fetches current request oAuth token
        /// </summary>
        /// <returns></returns>
        string GetRequestOAuthToken();
    }
}