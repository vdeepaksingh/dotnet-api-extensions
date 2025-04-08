namespace dotnet_api_extensions.Authentication
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    /// <param name="value"></param>
    public class OAuthTokenWrapper(string value)
    {

        /// <summary>
        /// 
        /// </summary>
        public string Value { get; internal set; } = value;
    }
}
