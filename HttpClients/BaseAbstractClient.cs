using dotnet_api_extensions.HttpClients.MessageMetadata;
using dotnet_api_extensions.Request;

namespace dotnet_api_extensions.HttpClients
{
    /// <summary>
    /// Abstract client for baseclient and baseprotobufclient classes
    /// </summary>
    /// <remarks>
    /// Default constructor
    /// </remarks>
    /// <param name="logger"></param>
    /// <param name="client"></param>
    /// <param name="requestContextAccessor"></param>
    public abstract class BaseAbstractClient(ILogger logger, HttpClient client, IRequestContextAccessor requestContextAccessor)
    {
        /// <summary>
        /// Logger
        /// </summary>
        protected internal readonly ILogger _logger = logger;

        /// <summary>
        /// Httpclient object
        /// </summary>
        protected internal readonly HttpClient _httpClient = client;

        /// <summary>
        /// Execute a get async call to requestUrl
        /// </summary>
        /// <typeparam name="TResponse">Type of the response object</typeparam>
        /// <param name="requestUrl"></param>
        /// <param name="requestMessageAddOnsOverride"></param>
        /// <returns></returns>
        public abstract Task<TResponse> ExecuteGetAsync<TResponse>(string requestUrl, Action<HttpRequestMessage> requestMessageAddOnsOverride = null);

        /// <summary>
        /// Execute a post async call to url with request object
        /// </summary>
        /// <typeparam name="TRequest">Type of the request object</typeparam>
        /// <typeparam name="TResponse">Type of the response object</typeparam>
        /// <param name="url"></param>
        /// <param name="request"></param>
        /// <param name="requestMessageAddOnsOverride"></param>
        /// <returns></returns>
        public abstract Task<TResponse> ExecutePostAsync<TRequest, TResponse>(string url, TRequest request, Action<HttpRequestMessage> requestMessageAddOnsOverride = null) where TRequest : class;

        /// <summary>
        /// Add information to request message.
        /// By default, it adds auth cookie or oAuth token
        /// </summary>
        /// <param name="message"></param>
        protected virtual IInfo RequestMessageAddOns(HttpRequestMessage message)
        {
            message.AddAuthCookie(requestContextAccessor);
            message.AddOAuthToken(requestContextAccessor);
            return null;
        }

        /// <summary>
        /// Adds information to request message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="addOns"></param>
        protected IInfo RequestMessageAddOns(HttpRequestMessage message, Action<HttpRequestMessage> addOns)
        {
            addOns?.Invoke(message);
            //Also execute default one
            return RequestMessageAddOns(message);
        }
    }
}