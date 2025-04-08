namespace dotnet_api_extensions.Cors
{
    /// <summary>
    /// All the 'out-of-the-box' types of cors policies
    /// </summary>
    public class CorsPolicyType
    {
        /// <summary>
        /// Used for allowing all the origins, methods and headers
        /// </summary>
        public const string AllowAny = nameof(AllowAny);

        /// <summary>
        /// Used for allowing specific origins
        /// </summary>
        public const string SpecificOrigins = nameof(SpecificOrigins);

        /// <summary>
        /// Used for allowing specific methods
        /// </summary>
        public const string SpecificMethods = nameof(SpecificMethods);
    }
}
