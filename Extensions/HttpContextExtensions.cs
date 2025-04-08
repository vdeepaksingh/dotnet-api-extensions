using dotnet_api_extensions.Authentication;
using dotnet_api_extensions.Cookie;

namespace dotnet_api_extensions.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Return name of the current auth cookie
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static string GetCurrentAuthCookieName(this IConfiguration configuration)
        {
            var authSectionExists = configuration.GetChildren().Any(item => item.Key == AuthenticationConstants.AuthenticationSection);

            if (!authSectionExists) return string.Empty;

            var authenticationSection = configuration.GetSection(AuthenticationConstants.AuthenticationSection);

            var cookieName = authenticationSection[AuthenticationConstants.CookieName];
            if (string.IsNullOrEmpty(cookieName)) cookieName = AuthenticationConstants.DefaultCookieName;

            return cookieName;
        }

        /// <summary>
        /// Return name of the current auth cookie
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static string GetCurrentAuthCookieName(this HttpContext httpContext)
        {
            if (!(httpContext.RequestServices.GetService(typeof(IConfiguration)) is IConfiguration configuration)) return string.Empty;

            return configuration.GetCurrentAuthCookieName();
        }

        /// <summary>
        /// Reads cookie from current httpContext by cookieName
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="cookieName"></param>
        /// <returns></returns>
        public static HttpCookieWrapper GetCookieFromRequest(this HttpContext httpContext, string cookieName)
        {
            try
            {
                string cookieValue = null;
                if (!string.IsNullOrEmpty(cookieName))
                {
                    cookieValue = CookieManager.GetRequestCookie(httpContext, cookieName);
                    if (string.IsNullOrEmpty(cookieValue))
                    {
                        cookieValue = null;
                    }
                }
                return new HttpCookieWrapper(cookieName, cookieValue);
            }
            catch (Exception)
            {
                //Eat up exception silently
                return new HttpCookieWrapper(cookieName, null);
            }
        }

        /// <summary>
        /// Reads auth cookie from request
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="authCookieName"></param>
        /// <returns></returns>
        public static HttpCookieWrapper GetAuthCookieFromRequest(this HttpContext httpContext, string authCookieName = AuthenticationConstants.DefaultCookieName)
        {
            return httpContext.GetCookieFromRequest(authCookieName);
        }

        /// <summary>
        /// Reads oAuth token from current httpContext
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static string GetOAuthTokenFromRequest(this HttpContext httpContext)
        {
            try
            {
                var authorizeHeader = httpContext.Request.Headers["Authorization"];
                if (authorizeHeader != Microsoft.Extensions.Primitives.StringValues.Empty)
                {
                    var token = authorizeHeader.FirstOrDefault();
                    if (token?.StartsWith("Bearer", StringComparison.OrdinalIgnoreCase) ?? false)
                    {
                        return token;
                    }
                }
            }
            catch (Exception)
            {
                //Eat up exception silently
            }
            return string.Empty;
        }

    }
}