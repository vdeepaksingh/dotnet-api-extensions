namespace dotnet_api_extensions.User
{
    public class UserInfo
    {
        /// <summary>
        /// UserId in form of GUID
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// User Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// property to get allowed user actions
        /// </summary>
        /// <value>The allowed action list</value>
        public HashSet<UserAction> AllowedActions { get; set; } = [];

        /// <summary>
        /// Roles associated with user
        /// </summary>
        public IList<RoleInfo> Roles { get; set; }
    }
}