namespace dotnet_api_extensions.HttpClients
{
    /// <summary>
    /// Adding a message handler to http client pipeline.
    /// Ideas from: https://thomaslevesque.com/2018/02/25/better-timeout-handling-with-httpclient/
    /// and https://thomaslevesque.com/2016/12/08/fun-with-the-httpclient-pipeline/
    /// </summary>
    internal class TimeoutHandler : DelegatingHandler
    {
        /// <summary>
        /// Default timeout of httprequestmessage.
        /// It's value is same as the default time out of httpClient 
        /// https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient.timeout?view=netcore-3.1
        /// </summary>
        public readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(100);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            using var cts = GetCancellationTokenSource(request, cancellationToken);
            try
            {
                return await base.SendAsync(
                    request,
                    cts?.Token ?? cancellationToken);
            }
            catch (OperationCanceledException)
                when (!cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException();
            }
        }

        private CancellationTokenSource GetCancellationTokenSource(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var timeout = request.GetTimeout() ?? DefaultTimeout;
            if (timeout == Timeout.InfiniteTimeSpan)
            {
                // No need to create a CTS if there's no timeout
                return null;
            }
            else
            {
                var cts = CancellationTokenSource
                    .CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(timeout);
                return cts;
            }
        }
    }
}