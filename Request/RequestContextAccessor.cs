using dotnet_api_extensions.Authentication;
using dotnet_api_extensions.User;

namespace dotnet_api_extensions.Request
{
    /// <summary>
    /// Stores request context in async local.
    /// Idea from: https://github.com/aspnet/HttpAbstractions/blob/master/src/Microsoft.AspNetCore.Http/HttpContextAccessor.cs
    /// </summary>
    internal class RequestContextAccessor : IRequestContextAccessor
    {
        private readonly AsyncLocal<HttpCookieWrapper> _requestHttpCookie = new();
        private readonly AsyncLocal<OAuthTokenWrapper> _requestOAuthToken = new();
        private readonly AsyncLocal<UserInfo> _loggedInUser = new();

        internal string AuthCookieName { get; set; }

        /// <summary>
        /// Stores current request auth cookie
        /// </summary>
        /// <param name="httpCookieWrapper"></param>
        public void StoreRequestAuthCookie(HttpCookieWrapper httpCookieWrapper)
        {
            if (_requestHttpCookie.Value == null) _requestHttpCookie.Value = httpCookieWrapper;
            else
            {
                _requestHttpCookie.Value.Name = httpCookieWrapper.Name;
                _requestHttpCookie.Value.Value = httpCookieWrapper.Value;
            }
        }

        /// <summary>
        /// Fetches current request auth cookie from asyncLocal store
        /// </summary>
        /// <returns></returns>
        public HttpCookieWrapper GetRequestAuthCookie()
        {
            return _requestHttpCookie.Value;
        }

        /// <summary>
        /// Stores current request oAuth token
        /// </summary>
        /// <param name="oAuthToken"></param>
        public void StoreRequestOAuthToken(string oAuthToken)
        {
            if (_requestOAuthToken.Value == null) _requestOAuthToken.Value = new OAuthTokenWrapper(oAuthToken);
            else
            {
                _requestOAuthToken.Value.Value = oAuthToken;
            }
        }

        /// <summary>
        /// Fetches current request oAuth token from asyncLocal store
        /// </summary>
        /// <returns></returns>
        public string GetRequestOAuthToken()
        {
            return _requestOAuthToken.Value?.Value;
        }

        /// <summary>
        /// Stores logged in user details
        /// </summary>
        /// <param name="userInfo"></param>
        public void StoreLoggedInUser(UserInfo userInfo)
        {
            _loggedInUser.Value = userInfo;
        }

        /// <summary>
        /// Fetches current logged in user from asyncLocal store
        /// </summary>
        /// <returns></returns>
        public UserInfo GetLoggedInUser()
        {
            return _loggedInUser.Value;
        }
    }
}