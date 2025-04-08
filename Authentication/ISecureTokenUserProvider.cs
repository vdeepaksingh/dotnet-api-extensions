using dotnet_api_extensions.User;

namespace dotnet_api_extensions.Authentication
{
    /// <summary>
    /// Reads "X-SecureTokenUser" header from request and provides user info stored in it
    /// </summary>
    public interface ISecureTokenUserProvider
    {
        /// <summary>
        /// Fetch user info
        /// </summary>
        /// <returns></returns>
        Task<UserInfo> GetUser();
    }
}