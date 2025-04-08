namespace dotnet_api_extensions.Authentication
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public class HttpCookieWrapper(string name, string value)
    {

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; internal set; } = name;

        /// <summary>
        /// 
        /// </summary>
        public string Value { get; internal set; } = value;
    }
}
