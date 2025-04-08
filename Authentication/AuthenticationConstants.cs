namespace dotnet_api_extensions.Authentication
{
    /// <summary>
    /// Constants for authentication flow
    /// </summary>
    public class AuthenticationConstants
    {
        /// <summary>
        /// Default cookie name
        /// </summary>
        public const string DefaultCookieName = ".AspNet.ApplicationCookie";

        /// <summary>
        /// Section name for authentication values
        /// </summary>
        public const string AuthenticationSection = "Authentication";

        /// <summary>
        /// Data protection purposes to be used for dataprotection.Protect and dataprotection.Unprotect
        /// </summary>
        public const string CommaSeparatedDataProtectionPurposes = "CommaSeparatedDataProtectionPurposes";

        /// <summary>
        /// 
        /// </summary>
        public const string AuthenticationScheme = "Scheme";

        /// <summary>
        /// 
        /// </summary>
        public const string CookieName = "CookieName";

        /// <summary>
        /// 
        /// </summary>
        public const string ExpireTimeInMins = "ExpireTimeInMins";

        /// <summary>
        /// 
        /// </summary>
        public const string CookieDomain = "CookieDomain";

        /// <summary>
        /// 
        /// </summary>
        public static readonly IList<string> DefaultDataProtectionPurposes =
            new List<string> { "CookieAuthenticationMiddleware", "ApplicationCookie", "v2" };

        /// <summary>
        /// Default authentication scheme
        /// </summary>
        public const string DefaultAuthenticationScheme = "ApplicationCookie";


        #region Token based authentication constants

        /// <summary>
        /// 
        /// </summary>
        public static readonly IList<string> DefaultTokenDataProtectionPurposes =
            new List<string> { "OAuthBearerTokenMiddleware" };

        /// <summary>
        /// Data protection purposes to be used for dataprotection.Protect and dataprotection.Unprotect for OAuth
        /// </summary>
        public const string CommaSeparatedOAuthDataProtectionPurposes = "CommaSeparatedOAuthDataProtectionPurposes";

        #endregion
    }
}