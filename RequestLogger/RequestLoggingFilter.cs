using dotnet_api_extensions.ApiLogger;

using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;

using Serilog;

namespace dotnet_api_extensions.RequestLogger
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="backgroundWorker"></param>
    /// <param name="diagnosticContext"></param>
    internal class RequestLoggingFilter(IApiLogWriter backgroundWorker,
        IDiagnosticContext diagnosticContext) : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var logEvent = new ApiLogEvent()
            {
                Url = context.HttpContext.Request.GetDisplayUrl(),
            };

            // By writing the file path to DiagnosticContext, we can read it in Serilog's request logger
            diagnosticContext.Set("DetailsFilePath", logEvent.FilePath);

            //Model binding would have happened by now. So,reading arguments directly
            var requestBody = context.ActionArguments;

            logEvent.RequestBody = requestBody;

            var executedContext = await next();

            logEvent.ResponseBody = executedContext.Result;

            backgroundWorker.LogEvent(logEvent);
        }
    }
}