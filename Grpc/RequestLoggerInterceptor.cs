using dotnet_api_extensions.ApiLogger;

using Grpc.Core;
using Grpc.Core.Interceptors;

using Serilog;

namespace dotnet_api_extensions.Grpc
{
    /// <summary>
    /// Interceptor for logging requests and responses
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    /// <param name="logger"></param>
    /// <param name="apiLogWriter"></param>
    /// <param name="diagnosticContext"></param>
    public class RequestLoggerInterceptor(ILogger<RequestLoggerInterceptor> logger,
        IApiLogWriter apiLogWriter,
        IDiagnosticContext diagnosticContext) : Interceptor
    {

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="continuation"></param>
        /// <returns></returns>
        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request,
                                          ServerCallContext context,
                                          UnaryServerMethod<TRequest, TResponse> continuation)
        {
            var apiEvent = new ApiLogEvent
            {
                Url = context.Host,
                RequestBody = request
            };

            // By writing the file path to DiagnosticContext, we can read it in Serilog's request logger
            diagnosticContext.Set("DetailsFilePath", apiEvent.FilePath);

            try
            {
                var response = await continuation(request, context);
                apiEvent.ResponseBody = response;
                apiEvent.StatusCode = context.Status.StatusCode.ToString();
                return response;
            }
            catch (RpcException ex)
            {
                logger.LogError(ex, "{summary}", "Encountered exception");
                apiEvent.ResponseBody = ex.Message;
                apiEvent.StatusCode = ex.StatusCode.ToString();
                throw;
            }
            finally
            {
                apiLogWriter.LogEvent(apiEvent);
            }
        }
    }
}