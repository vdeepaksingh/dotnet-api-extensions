namespace dotnet_api_extensions.EndPoints
{
    /// <summary>
    /// An interface as a contract for EndPointMiddleware. 
    /// Applications do not need to implement it explicity until their middleware has "public Task &lt;object&gt;  GetResultAsync()"
    /// or "public object GetResult()"
    /// </summary>
    public interface IEndPointMiddleware
    {
        /// <summary>
        /// Contract method which returns results asynchronously
        /// </summary>
        /// <returns></returns>
        Task<object> GetResultAsync();
    }

    internal interface IEndPointMiddlewareInternal : IEndPointMiddleware
    {
        /// <summary>
        /// Path of the invoked middleware
        /// </summary>
        void SetPath(string path);
    }
}
