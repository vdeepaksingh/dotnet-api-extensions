using System.Net;
using System.Text;

using dotnet_api_extensions.HttpClients.MessageMetadata;
using dotnet_api_extensions.Request;

using Newtonsoft.Json;

namespace dotnet_api_extensions.HttpClients
{
    /// <summary>
    /// A base client to be used for Get and Post async calls
    /// This class is a wrapper over HttpClient
    /// </summary>
    /// <remarks>
    /// Default constructor
    /// </remarks>
    /// <param name="logger"></param>
    /// <param name="client"></param>
    /// <param name="requestContextAccessor"></param>
    public class BaseClient(ILogger<BaseClient> logger, HttpClient client, IRequestContextAccessor requestContextAccessor) : BaseAbstractClient(logger, client, requestContextAccessor)
    {

        /// <summary>
        /// Execute a get async call to requestUrl
        /// </summary>
        /// <typeparam name="TResponse">Type of the response object</typeparam>
        /// <param name="requestUrl"></param>
        /// <param name="requestMessageAddOns"></param>
        /// <returns></returns>
        public override async Task<TResponse> ExecuteGetAsync<TResponse>(string requestUrl, Action<HttpRequestMessage> requestMessageAddOns = null)
        {
            try
            {
                _logger.LogDebug("{summary} {url}", "Executing url", requestUrl);

                var message = new HttpRequestMessage(HttpMethod.Get, requestUrl);

                var preCallInfo = RequestMessageAddOns(message, requestMessageAddOns);

                var response = await _httpClient.SendAsync(message);

                var responseContent = await response.Content.ReadAsStringAsync();

                TResponse responseObj = response.IsSuccessStatusCode ? DeserializeJson<TResponse>(responseContent) : default;

                var requestObjToBeLogged = preCallInfo is ExternalClientInfo ? HeadersAsStringDictionary(message) : null;

                if (responseObj is not null)
                {
                    ResponsePostProcessing(requestUrl, requestObjToBeLogged, response.StatusCode, responseObj, preCallInfo);
                    return responseObj;
                }
                else
                {
                    ResponsePostProcessing(requestUrl, requestObjToBeLogged, response.StatusCode, responseContent, preCallInfo);
                    throw new ServiceClientException($"API Exception - Response could not be processed. Status code ={response.StatusCode}");
                }
            }
            catch (Exception ex) when (ex is not ServiceClientException)
            {
                _logger.LogError(ex, "{summary} to {url}", "Error executing GET", requestUrl);
                throw new ServiceClientException(ex);
            }
        }

        /// <summary>
        /// Execute a post async call to url with request object
        /// </summary>
        /// <typeparam name="TRequest">Type of the request object</typeparam>
        /// <typeparam name="TResponse">Type of the response object</typeparam>
        /// <param name="url"></param>
        /// <param name="request"></param>
        /// <param name="requestMessageAddOns"></param>
        /// <returns></returns>
        public override async Task<TResponse> ExecutePostAsync<TRequest, TResponse>(string url, TRequest request, Action<HttpRequestMessage> requestMessageAddOns = null) where TRequest : class
        {
            var jsonRequest = JsonConvert.SerializeObject(request);

            _logger.LogDebug("{summary} - {url} | Request - {request}", "Executing Post Async", url, jsonRequest);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            try
            {
                var message = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };

                var preCallInfo = RequestMessageAddOns(message, requestMessageAddOns);

                var response = await _httpClient.SendAsync(message);

                var responseContent = await response.Content.ReadAsStringAsync();

                TResponse responseObj = response.IsSuccessStatusCode ? DeserializeJson<TResponse>(responseContent) : default;

                var requestObjToBeLogged = preCallInfo is ExternalClientInfo ? new { headers = HeadersAsStringDictionary(message), request } : null;

                if(responseObj is not null)
                {
                    ResponsePostProcessing(url, requestObjToBeLogged, response.StatusCode, responseObj, preCallInfo);
                    return responseObj;
                }
                else
                {
                    ResponsePostProcessing(url, requestObjToBeLogged, response.StatusCode, responseContent, preCallInfo);
                    throw new ServiceClientException($"API Exception - Response could not be processed. Status code ={response.StatusCode}");
                }
            }
            catch (Exception ex) when (ex is not ServiceClientException)
            {
                _logger.LogError(ex, "{summary} to {url}", "Error executing POST", url);
                throw new ServiceClientException(ex);
            }
        }

        /// <summary>
        /// Post process response message
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="requestBody"></param>
        /// <param name="statusCode"></param>
        /// <param name="responseBody"></param>
        /// <param name="requestInfo"></param>
        protected virtual void ResponsePostProcessing(string requestUrl, object requestBody, HttpStatusCode statusCode, object responseBody, IInfo requestInfo)
        {
            //DO NOTHING
        }

        private static TResponse DeserializeJson<TResponse>(string responseContent)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(responseContent))
                {
                    return JsonConvert.DeserializeObject<TResponse>(responseContent);
                }
                else
                {
                    return default;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error While deserializing object ", ex);
            }
        }

        private static Dictionary<string, string> HeadersAsStringDictionary(HttpRequestMessage requestMessage)
            => requestMessage.Headers.ToDictionary(a => a.Key, a => string.Join(";", a.Value));
    }
}