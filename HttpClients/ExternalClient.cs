using System.Diagnostics;
using System.Net;

using dotnet_api_extensions.ApiLogger;
using dotnet_api_extensions.HttpClients.MessageMetadata;

namespace dotnet_api_extensions.HttpClients
{
    /// <summary>
    /// Calls to external services should be made through this
    /// </summary>
    /// <remarks>
    /// Default constructor
    /// </remarks>
    /// <param name="logger"></param>
    /// <param name="client"></param>
    /// <param name="apiLogWriter"></param>
    public class ExternalClient(ILogger<ExternalClient> logger, HttpClient client, IApiLogWriter apiLogWriter) : BaseClient(logger, client, null)
    {
        private bool _apiLogsDisabled;

        /// <summary>
        /// Disable api logging if it is going to take too much space
        /// </summary>
        public void DisableApiLogs()
        {
            _apiLogsDisabled = true;
        }

        /// <summary>
        /// Set LogLevel for logging. Default is Information
        /// </summary>
        public LogLevel LogLevel { get; set; } = LogLevel.Information;

        /// <summary>
        /// Add ons for request message
        /// </summary>
        /// <param name="message"></param>
        protected override IInfo RequestMessageAddOns(HttpRequestMessage message)
        {
            //Override so that auth cookie is not added to the request message

            //Also start stopwatch for elapsedTime logging
            return new ExternalClientInfo { CallStartTime = Stopwatch.GetTimestamp() };
        }

        private static double GetElapsedMilliseconds(long start, long stop)
        {
            return (stop - start) * 1000 / (double)Stopwatch.Frequency;
        }

        /// <summary>
        /// Post process response message
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="jsonRequest"></param>
        /// <param name="statusCode"></param>
        /// <param name="jsonResponse"></param>
        /// <param name="requestInfo"></param>
        protected override void ResponsePostProcessing(string requestUrl, object jsonRequest, HttpStatusCode statusCode, object jsonResponse, IInfo requestInfo)
        {
            if (requestInfo is not ExternalClientInfo externalClientInfo) return;

            var elapsedTime = GetElapsedMilliseconds(externalClientInfo.CallStartTime, Stopwatch.GetTimestamp());

            if (_apiLogsDisabled)
            {
                _logger.Log(LogLevel, "External call to {url} responded {StatusCode} in {Elapsed:0.0000} ms", requestUrl, statusCode, elapsedTime);
            }
            else
            {
                var apiLogEvent = new ApiLogEvent
                {
                    Url = requestUrl,
                    RequestBody = jsonRequest,
                    StatusCode = statusCode.ToString(),
                    ResponseBody = jsonResponse
                };
                apiLogWriter.LogEvent(apiLogEvent);

                _logger.Log(LogLevel, "External call to {url} responded {StatusCode} in {Elapsed:0.0000} ms {DetailsFilePath}", requestUrl, statusCode, elapsedTime, apiLogEvent.FilePath);
            }
        }
    }
}