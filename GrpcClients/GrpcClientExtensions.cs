using System.Net;

using dotnet_api_extensions.Authentication;
using dotnet_api_extensions.HttpClients;
using dotnet_api_extensions.Request;

using Grpc.Core;
using Grpc.Net.Client.Web;

using Polly;

using ProtoBuf.Grpc.ClientFactory;

namespace dotnet_api_extensions.GrpcClients
{
    /// <summary>
    /// 
    /// </summary>
    public static class GrpcClientExtensions
    {
        private static readonly HttpStatusCode[] ServerErrors = new HttpStatusCode[] {
                HttpStatusCode.BadGateway,
                HttpStatusCode.GatewayTimeout,
                HttpStatusCode.ServiceUnavailable,
                HttpStatusCode.InternalServerError,
                HttpStatusCode.TooManyRequests,
                HttpStatusCode.RequestTimeout
            };

        private static readonly StatusCode[] GRpcErrors = new StatusCode[] {
                StatusCode.DeadlineExceeded,
                StatusCode.Internal,
                StatusCode.NotFound,
                StatusCode.ResourceExhausted,
                StatusCode.Unavailable,
                StatusCode.Unknown
            };

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="services"></param>
        /// <param name="uri"></param>
        /// <param name="retryCount"></param>
        /// <param name="handledEventsAllowedBeforeBreaking"></param>
        /// <param name="durationOfCircuitBreakInSeconds"></param>
        /// <param name="bypassSslCheck"></param>
        /// <returns></returns>
        public static IServiceCollection RegisterGrpcClient<TImplementation>(
            this IServiceCollection services,
            string uri,
            int retryCount = 3,
            int handledEventsAllowedBeforeBreaking = 3,
            int durationOfCircuitBreakInSeconds = 30,
            bool bypassSslCheck = false
            )
            where TImplementation : class
        {
            services.AddRequestContextAccessor();

            services.AddCodeFirstGrpcClient<TImplementation>(o => o.Address = new Uri(uri))
                .ConfigureChannel((serviceProvider, channelOptions) =>
                {
                    (var authCookie, var oAuthToken) = GetAuthInfo<TImplementation>(serviceProvider);

                    if (string.IsNullOrEmpty(authCookie?.Value) && string.IsNullOrEmpty(oAuthToken)) return;

                    channelOptions.Credentials = GetCredentials(authCookie,oAuthToken);
                }).AddHandlerAndPolicies(retryCount, handledEventsAllowedBeforeBreaking, durationOfCircuitBreakInSeconds, bypassSslCheck);

            return services;
        }

        private static (HttpCookieWrapper Cookie, string OAuthToken) GetAuthInfo<T>(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null) return (null,null);

            if (serviceProvider.GetService(typeof(IRequestContextAccessor)) is IRequestContextAccessor requestContextAccessor)
            {
                var cookie = requestContextAccessor.GetRequestAuthCookie();
                if (!string.IsNullOrEmpty(cookie?.Value))
                {
                    if (serviceProvider.GetService(typeof(ILogger<T>)) is ILogger logger)
                    {
                        logger.LogDebug("Reading auth cookie {cookieName} from request context.", cookie.Name);
                    }
                }
                return (cookie, requestContextAccessor.GetRequestOAuthToken());
            }

            return (null,null);
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

                    return new GrpcWebHandler(handler);
                });

            httpClientBuilder.AddHttpMessageHandler(() => new TimeoutHandler());

            return httpClientBuilder
                .AddPolicyHandler(GetRetryPolicy(retryCount))
                .AddPolicyHandler(GetCircuitBreakerPatternPolicy(handledEventsAllowedBeforeBreaking, durationOfCircuitBreakInSeconds));
        }

        private static ChannelCredentials GetCredentials(HttpCookieWrapper authCookie, string oAuthToken)
        {
            var credentials = CallCredentials.FromInterceptor((context, metadata) =>
            {
                if (!string.IsNullOrEmpty(authCookie?.Value))
                {
                    metadata.Add("Cookie", $"{authCookie.Name}={authCookie.Value}");
                }
                else if (!string.IsNullOrEmpty(oAuthToken))
                {
                    metadata.Add("Authorization", oAuthToken);
                }

                return Task.CompletedTask;
            });

            // SslCredentials is used here because this channel is using TLS.
            // CallCredentials can't be used with ChannelCredentials.Insecure on non-TLS channels.
            return ChannelCredentials.Create(new SslCredentials(), credentials);
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
            return Policy.HandleResult<HttpResponseMessage>(r =>
            {
                var grpcStatus = StatusManager.GetStatusCode(r);
                var httpStatusCode = r.StatusCode;

                return grpcStatus == null && ServerErrors.Contains(httpStatusCode) || // if the server send an error before gRPC pipeline
                       httpStatusCode == HttpStatusCode.OK && GRpcErrors.Contains(grpcStatus.Value); // if gRPC pipeline handled the request (gRPC always answers OK)
            });
        }
    }
}