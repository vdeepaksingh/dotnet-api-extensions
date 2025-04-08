namespace dotnet_api_extensions.User
{
    public interface IUserInfos
    {
        /// <summary>
        /// Get user data for provided userId
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<UserInfo> Get(string userId);

        /// <summary>
        /// Gets list of user data for email ids
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<IList<UserInfo>> GetByEmailIds(IList<string> emailIds);
    }
}
