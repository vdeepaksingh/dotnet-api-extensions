using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;

using System.Text;

namespace dotnet_api_extensions.Authentication
{
    /// <summary>
    /// Idea from: https://social.msdn.microsoft.com/Forums/en-US/d1543d05-c5c4-43f2-a43c-cdfad2d40f89/how-to-share-bearer-tokens-between-aspnet-4x-and-aspnet-5-applications
    /// </summary>
    internal class OAuthBearerTokenMiddleware(RequestDelegate next,
        ILogger<OAuthBearerTokenMiddleware> logger,
        IDataProtectionProvider dataProtectionProvider)
    {
        private const string BearerIdentifier = "Bearer";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context, IConfiguration configuration)
        {
            if (!context.User.Identity.IsAuthenticated)
            {
                try
                {
                    var authorizeHeader = context.Request.Headers["Authorization"];

                    if (authorizeHeader != Microsoft.Extensions.Primitives.StringValues.Empty)
                    {
                        var token = authorizeHeader.FirstOrDefault();
                        if (token?.StartsWith(BearerIdentifier, StringComparison.OrdinalIgnoreCase) ?? false)
                        {
                            token = token.Replace(BearerIdentifier, string.Empty).Trim();

                            var dataProtectionPurposes = configuration.GetAuthenticationSection().GetOAuthProtectionPurposes();

                            var dataProtector = dataProtectionProvider.CreateProtector(dataProtectionPurposes);
                            var jsonResult = dataProtector.Unprotect(token);

                            var serializer = new TicketSerializer();
                            var ticket = serializer.Deserialize(Encoding.UTF8.GetBytes(jsonResult));
                            context.User = ticket.Principal;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed in OAuthBearerTokenMiddleware");
                }
            }

            await next(context);
        }
    }
}
