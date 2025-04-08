namespace dotnet_api_extensions.EndPoints
{
    /// <summary>
    /// This handles end point middlwares which are used for dynamic end points.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Default constructor
    /// </remarks>
    /// <param name="logger"></param>
    public class EndPointMiddlewareHandler<T>(ILogger<T> logger)
    {
        private readonly ILogger _logger = logger;

        private async Task<object> ExecuteDynamic(object endPointMiddlewareInstance)
        {
            var asyncMethod = typeof(T).GetMethod("GetResultAsync");

            if (asyncMethod != null)
            {
                return await (Task<object>)asyncMethod.Invoke(endPointMiddlewareInstance, [])!;
            }

            var syncMethod = typeof(T).GetMethod("GetResult");

            if (syncMethod != null)
            {
                return syncMethod.Invoke(endPointMiddlewareInstance, [])!;
            }

            //If needed methods not found, then log error
            _logger.LogError("{summary} in {fullName}", "Neither GetResultAsync nor GetResult method found", typeof(T).FullName);
            return null!;
        }

        /// <summary>
        /// Handles the request for a specific path by writing the result from middleware to HttpContext.Response.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task HandleRequest(HttpContext context, string path)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var endPointMiddlewareInstance = context.RequestServices.GetService(typeof(T));

            if (endPointMiddlewareInstance != null)
            {
                try
                {
                    var result = endPointMiddlewareInstance switch
                    {
                        IEndPointMiddlewareInternal internalInterfaceMiddleware => await GetResultAsync(internalInterfaceMiddleware, path),
                        IEndPointMiddleware interfaceMiddleware => await interfaceMiddleware.GetResultAsync(),
                        _ => await ExecuteDynamic(endPointMiddlewareInstance)
                    };

                    await context.GetNoCacheResponseAsync(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{summary} for path {path}", "Failed to render result", path);
                    await context.GetNoCacheResponseAsync("Failed");
                }
            }
            else
            {
                _logger.LogError("{summary}", "Instance of end point middleware not found");
                await context.GetNoCacheResponseAsync("Failed");
            }
        }

        private async Task<object> GetResultAsync(IEndPointMiddlewareInternal internalInterfaceMiddleware, string path)
        {
            internalInterfaceMiddleware.SetPath(path);
            return await internalInterfaceMiddleware.GetResultAsync();
        }
    }
}