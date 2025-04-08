using System.Net;

using dotnet_api_extensions.ApiLogger;
using dotnet_api_extensions.Request;

using Polly;

namespace dotnet_api_extensions.HttpClients
{
    /// <summary>
    /// Extensions for Http client with Polly's retry policy and circuit breaker
    /// </summary>
    public static class HttpClientExtensions
    {

        /// <summary>
        /// Registers baseclient object with transient error handling
        /// </summary>
        /// <param name="services"></param>
        /// <param name="retryCount"></param>
        /// <param name="handledEventsAllowedBeforeBreaking"></param>
        /// <param name="durationOfCircuitBreakInSeconds"></param>
        /// <param name="bypassSslCheck">For development environment, you need to bypass SSL check</param>
        /// <returns></returns>
        public static IServiceCollection RegisterBaseClient(
            this IServiceCollection services,
            int retryCount = 3,
            int handledEventsAllowedBeforeBreaking = 3,
            int durationOfCircuitBreakInSeconds = 30,
            bool bypassSslCheck = false)
        {
            services.AddRequestContextAccessor();
            return services.RegisterHttpClient<BaseClient>(retryCount, handledEventsAllowedBeforeBreaking, durationOfCircuitBreakInSeconds, bypassSslCheck);
        }

        /// <summary>
        /// Registers baseclient object with transient error handling
        /// </summary>
        /// <param name="services"></param>
        /// <param name="retryCount"></param>
        /// <param name="handledEventsAllowedBeforeBreaking"></param>
        /// <param name="durationOfCircuitBreakInSeconds"></param>
        /// <param name="bypassSslCheck">For development environment, you need to bypass SSL check</param>
        /// <returns></returns>
        public static IServiceCollection RegisterExternalClient(
            this IServiceCollection services,
            int retryCount = 3,
            int handledEventsAllowedBeforeBreaking = 3,
            int durationOfCircuitBreakInSeconds = 30,
            bool bypassSslCheck = false)
        {
            services.AddApiLogWriter();
            return services.RegisterHttpClient<ExternalClient>(retryCount, handledEventsAllowedBeforeBreaking, durationOfCircuitBreakInSeconds, bypassSslCheck);
        }

        /// <summary>
        /// Registers baseprotoBufclient object with transient error handling
        /// </summary>
        /// <param name="services"></param>
        /// <param name="retryCount"></param>
        /// <param name="handledEventsAllowedBeforeBreaking"></param>
        /// <param name="durationOfCircuitBreakInSeconds"></param>
        /// <param name="bypassSslCheck">For development environment, you need to bypass SSL check</param>
        /// <returns></returns>
        public static IServiceCollection RegisterBaseProtoBufClient(
            this IServiceCollection services,
            int retryCount = 3,
            int handledEventsAllowedBeforeBreaking = 3,
            int durationOfCircuitBreakInSeconds = 30,
            bool bypassSslCheck = false)
        {
            services.AddRequestContextAccessor();
            return services.RegisterHttpClient<BaseProtoBufClient>(retryCount, handledEventsAllowedBeforeBreaking, durationOfCircuitBreakInSeconds, bypassSslCheck);
        }

        /// <summary>
        /// Registers an Http Client with transient error handling
        /// </summary>
        /// <typeparam name="TClient">Type of the typed client</typeparam>
        /// <typeparam name="TImplementation">Implementation of the typed client</typeparam>
        /// <param name="services"></param>
        /// <param name="retryCount"></param>
        /// <param name="handledEventsAllowedBeforeBreaking"></param>
        /// <param name="durationOfCircuitBreakInSeconds"></param>
        /// <param name="bypassSslCheck">For development environment, you need to bypass SSL check</param>
        /// <returns></returns>
        public static IServiceCollection RegisterHttpClient<TClient, TImplementation>(
            this IServiceCollection services,
            int retryCount = 3,
            int handledEventsAllowedBeforeBreaking = 3,
            int durationOfCircuitBreakInSeconds = 30,
            bool bypassSslCheck = false)
            where
            TClient : class
            where TImplementation : class, TClient
        {
            services.AddHttpClient<TClient, TImplementation>()
                .AddHandlerAndPolicies(retryCount, handledEventsAllowedBeforeBreaking, durationOfCircuitBreakInSeconds, bypassSslCheck);

            return services;
        }

        /// <summary>
        /// Registers an Http Client with transient error handling
        /// </summary>
        /// <typeparam name="TImplementation">Type and implementation of the typed client</typeparam>
        /// <param name="services"></param>
        /// <param name="retryCount"></param>
        /// <param name="handledEventsAllowedBeforeBreaking"></param>
        /// <param name="durationOfCircuitBreakInSeconds"></param>
        /// <param name="bypassSslCheck">For development environment, you need to bypass SSL check</param>
        /// <returns></returns>
        public static IServiceCollection RegisterHttpClient<TImplementation>(
            this IServiceCollection services,
            int retryCount = 3,
            int handledEventsAllowedBeforeBreaking = 3,
            int durationOfCircuitBreakInSeconds = 30,
            bool bypassSslCheck = false
            )
            where TImplementation : class
        {
            services.AddHttpClient<TImplementation>()
                 .AddHandlerAndPolicies(retryCount, handledEventsAllowedBeforeBreaking, durationOfCircuitBreakInSeconds, bypassSslCheck);

            return services;
        }

        private static IHttpClientBuilder AddHandlerAndPolicies(this IHttpClientBuilder httpClientBuilder,
            int retryCount,
            int handledEventsAllowedBeforeBreaking,
            int durationOfCircuitBreakInSeconds,
            bool bypassSslCheck)
        {
            httpClientBuilder.ConfigurePrimaryHttpMessageHandler(
                () =>
                {
                    var handler = new HttpClientHandler();
                    if (bypassSslCheck)
                    {
                        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                    }

                    //To disable cookieContainer so that individual cookies can be configured at run time
                    handler.UseCookies = false;
                    return handler;
                });

            httpClientBuilder.AddHttpMessageHandler(() => new TimeoutHandler());

            return httpClientBuilder
                .AddPolicyHandler(GetRetryPolicy(retryCount))
                .AddPolicyHandler(GetCircuitBreakerPatternPolicy(handledEventsAllowedBeforeBreaking, durationOfCircuitBreakInSeconds));
        }

        /// <summary>
        /// Exponential Retry Policy (Provided by Polly)
        /// </summary>
        /// <returns></returns>
        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount)
        {
            return GetPolicyBuilder()
                .WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        /// <summary>
        /// Circuit Breaker Policy (Provided by Polly)
        /// </summary>
        /// <returns></returns>
        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPatternPolicy(
            int handledEventsAllowedBeforeBreaking,
            int durationOfCircuitBreakInSeconds)
        {
            return GetPolicyBuilder()
                .CircuitBreakerAsync(handledEventsAllowedBeforeBreaking, TimeSpan.FromSeconds(durationOfCircuitBreakInSeconds));
        }

        private static PolicyBuilder<HttpResponseMessage> GetPolicyBuilder()
        {
            return Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(TransientHttpStatusCodePredicate);
        }

        /// <summary>
        /// We handle all errors above error code 500 and Timeouts
        /// </summary>
        private static readonly Func<HttpResponseMessage, bool> TransientHttpStatusCodePredicate = (response) =>
        {
            return (int)response.StatusCode > 500
                || response.StatusCode == HttpStatusCode.RequestTimeout;
            //|| response.StatusCode == HttpStatusCode.NotFound;
        };

        /// <summary>
        /// Call GetAsync with specified timeOut
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="requestUri"></param>
        /// <param name="timeOutInSecs"></param>
        /// <returns></returns>
        public static Task<HttpResponseMessage> GetAsync(this HttpClient httpClient, string requestUri, int timeOutInSecs)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            return httpClient.SendAsync(request, timeOutInSecs);
        }

        /// <summary>
        /// Call PostAsync with specified timeOut
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="requestUri"></param>
        /// <param name="content"></param>
        /// <param name="timeOutInSecs"></param>
        /// <returns></returns>
        public static Task<HttpResponseMessage> PostAsync(this HttpClient httpClient, string requestUri, HttpContent content, int timeOutInSecs)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = content
            };
            return httpClient.SendAsync(request, timeOutInSecs);
        }

        /// <summary>
        /// Call SendAsync with specified timeout
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="request"></param>
        /// <param name="timeOutInSecs"></param>
        /// <returns></returns>
        public static Task<HttpResponseMessage> SendAsync(this HttpClient httpClient, HttpRequestMessage request, int timeOutInSecs)
        {
            var timeoutSpans = TimeSpan.FromSeconds(timeOutInSecs);
            request.SetTimeout(timeoutSpans);
            if (timeoutSpans > httpClient.Timeout) httpClient.Timeout = timeoutSpans.Add(TimeSpan.FromMinutes(1));
            return httpClient.SendAsync(request);
        }
    }
}