namespace dotnet_api_extensions.HttpClients
{
    //Idea from https://thomaslevesque.com/2018/02/25/better-timeout-handling-with-httpclient/
    /// <summary>
    /// 
    /// </summary>
    public static class HttpMessageTimeoutExtensions
    {
        private static readonly string TimeoutPropertyKey = "RequestTimeout";

        /// <summary>
        /// To set timeout to a http request message
        /// </summary>
        /// <param name="request"></param>
        /// <param name="timeout"></param>
        internal static void SetTimeout(
            this HttpRequestMessage request,
            TimeSpan? timeout)
        {
            ArgumentNullException.ThrowIfNull(request);
            request.Options.Set(new HttpRequestOptionsKey<TimeSpan?>(TimeoutPropertyKey), timeout);
        }

        /// <summary>
        /// To read timeout of a http request message
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static TimeSpan? GetTimeout(this HttpRequestMessage request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.Options.TryGetValue(
                    new HttpRequestOptionsKey<TimeSpan?>(TimeoutPropertyKey),
                    out var value)
                && value is TimeSpan timeout)
                return timeout;
            return null;
        }
    }
}