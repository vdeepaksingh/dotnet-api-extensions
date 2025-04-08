using dotnet_api_extensions.User;

using Microsoft.AspNetCore.DataProtection;

namespace dotnet_api_extensions.Authentication
{
    internal class SecureTokenUserProvider(IHttpContextAccessor httpContextAccessor,
        IDataProtectionProvider provider,
        IUserInfos userInfos,
        ILogger<SecureTokenUserProvider> logger) : ISecureTokenUserProvider
    {
        public async Task<UserInfo> GetUser()
        {
            string secureTokenUserHeader = httpContextAccessor.HttpContext?.Request.Headers["X-SecureTokenUser"];

            if (!string.IsNullOrWhiteSpace(secureTokenUserHeader))
            {
                var protectorPurposes = AuthenticationConstants.DefaultDataProtectionPurposes;
                var protector = provider.CreateProtector(protectorPurposes);

                try
                {
                    var userID = protector.Unprotect(secureTokenUserHeader);

                    if (!string.IsNullOrWhiteSpace(userID))
                    {
                        return await userInfos.Get(userID);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error in unprotecting secure user token {token}", secureTokenUserHeader);
                }
            }

            return null;
        }
    }
}