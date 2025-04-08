using dotnet_api_extensions.Extensions;
using dotnet_api_extensions.Request;
using dotnet_api_extensions.User;

using System.Security.Claims;

namespace dotnet_api_extensions.Authentication
{
    /// <summary>
    /// To save request context details
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    /// <param name="next"></param>
    /// <param name="requestContextAccessor"></param>
    internal class SaveRequestContextMiddleware(RequestDelegate next,
        RequestContextAccessor requestContextAccessor)
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userInfos"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context, IUserInfos userInfos)
        {
            if (context == null || !context.User.Identity.IsAuthenticated)
            {
                await next(context);
                return;
            }

            var authCookieName = requestContextAccessor.AuthCookieName;
            var authCookieWrapper = context.GetAuthCookieFromRequest(authCookieName);

            if (!string.IsNullOrEmpty(authCookieWrapper?.Value))
            {
                requestContextAccessor.StoreRequestAuthCookie(authCookieWrapper);
            }

            var oAuthToken = context.GetOAuthTokenFromRequest();

            if (!string.IsNullOrEmpty(oAuthToken))
            {
                requestContextAccessor.StoreRequestOAuthToken(oAuthToken);
            }

            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userIdClaim?.Value))
            {
                var user = await userInfos.Get(userIdClaim.Value);
                if (user != null)
                {
                    if (!string.IsNullOrWhiteSpace(context.User.Identity.Name))
                    {
                        user.Name = context.User.Identity.Name;
                    }
                    requestContextAccessor.StoreLoggedInUser(user);
                }
            }

            await next(context);
        }
    }
}