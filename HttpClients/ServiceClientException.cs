namespace dotnet_api_extensions.HttpClients
{
    /// <summary>
    /// Custom exception object to be wrapped over http client exceptions
    /// </summary>
    public class ServiceClientException : Exception
    {
        /// <summary>
        /// Constructor with exception object
        /// </summary>
        /// <param name="ex"></param>
        public ServiceClientException(Exception ex) : base(ex.Message, ex)
        {

        }

        /// <summary>
        /// Constructor with message string
        /// </summary>
        /// <param name="message"></param>
        public ServiceClientException(string message) : base(message)
        {

        }
    }
}
