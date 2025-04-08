using dotnet_api_extensions.Request;

using ProtoBuf;

using System.Net.Http.Headers;

namespace dotnet_api_extensions.HttpClients
{
    /// <summary>
    /// BaseClient for protobuf requests
    /// </summary>
    /// <remarks>
    /// Default constructor
    /// </remarks>
    /// <param name="logger"></param>
    /// <param name="client"></param>
    /// <param name="requestContextAccessor"></param>
    public class BaseProtoBufClient(ILogger<BaseProtoBufClient> logger,
                              HttpClient client,
                              IRequestContextAccessor requestContextAccessor) : BaseAbstractClient(logger, client, requestContextAccessor)
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

                var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-protobuf"));

                RequestMessageAddOns(request, requestMessageAddOns);

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseStream = await response.Content?.ReadAsStreamAsync();

                    return Serializer.Deserialize<TResponse>(responseStream);
                }
                else
                {
                    throw new ServiceClientException("API Exception - Response status code was not successful");
                }
            }
            catch (Exception ex) when (!(ex is ServiceClientException))
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
        /// <param name="requestUrl"></param>
        /// <param name="content"></param>
        /// <param name="requestMessageAddOns"></param>
        /// <returns></returns>
        public override async Task<TResponse> ExecutePostAsync<TRequest, TResponse>(string requestUrl, TRequest content, Action<HttpRequestMessage> requestMessageAddOns = null) where TRequest : class
        {
            try
            {
                _logger.LogDebug("{summary} {url}", "Executing url", requestUrl);

                var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-protobuf"));

                var byteArrayContent = new ByteArrayContent(ProtoSerialize(content));
                byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-protobuf");

                request.Content = byteArrayContent;

                RequestMessageAddOns(request, requestMessageAddOns);

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseStream = await response.Content?.ReadAsStreamAsync();

                    return Serializer.Deserialize<TResponse>(responseStream);
                }
                else
                {
                    throw new ServiceClientException("API Exception - Response status code was not successful");
                }
            }
            catch (Exception ex) when (!(ex is ServiceClientException))
            {
                _logger.LogError(ex, "{summary} to {url}", "Error executing POST", requestUrl);
                throw new ServiceClientException(ex);
            }
        }

        /// <summary>
        /// Serialize a record in proto-readable byte array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="record"></param>
        /// <returns></returns>
        public static byte[] ProtoSerialize<T>(T record) where T : class
        {
            using var stream = new MemoryStream();
            Serializer.Serialize(stream, record);
            return stream.ToArray();
        }
    }
}