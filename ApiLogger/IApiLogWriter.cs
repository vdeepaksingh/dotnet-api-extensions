namespace dotnet_api_extensions.ApiLogger
{
    /// <summary>
    /// Log writer for API
    /// </summary>
    public interface IApiLogWriter : IDisposable
    {
        /// <summary>
        /// Log an event
        /// </summary>
        /// <param name="logEvent"></param>
        void LogEvent(ApiLogEvent logEvent);
    }
}