using dotnet_api_extensions.ApiLogger;
using dotnet_api_extensions.Authentication;
using dotnet_api_extensions.Caches;
using dotnet_api_extensions.Cors;
using dotnet_api_extensions.DataProtection;
using dotnet_api_extensions.EndPoints;
using dotnet_api_extensions.FluentValidation;
using dotnet_api_extensions.Grpc;
using dotnet_api_extensions.HttpClients;
using dotnet_api_extensions.Request;
using dotnet_api_extensions.RequestLogger;
using dotnet_api_extensions.Response;
using dotnet_api_extensions.ServiceCollection;
using dotnet_api_extensions.Swagger;

using FluentValidation;
using FluentValidation.AspNetCore;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;

using ProtoBuf.Grpc.Server;

using Serilog;

using WebApiContrib.Core.Formatter.Protobuf;

using AssemblyExtensions = dotnet_api_extensions.Extensions.AssemblyExtensions;

namespace dotnet_api_extensions
{
    /// <summary>
    /// Extension methods for IWebHostBuilder
    /// </summary>
    public static class WebHostBuilderExtensions
    {
        /// <summary>
        /// All defaults at one place.
        /// Adds logging, grpc, Authentication, Swagger , HealthChecks, "Default" cors-policy, base http client, 
        /// cache store repo end point and dynamic end-points
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IWebHostBuilder UseDefaults(this IWebHostBuilder builder) => builder.UseDefaults(false);

        /// <summary>
        /// All defaults at one place.
        /// Adds logging, grpc, Authentication, Swagger , HealthChecks, "Default" cors-policy, base http client, 
        /// cache store repo end point and dynamic end-points
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="enableFluentValidationAspnetPipeline"></param>
        /// <returns></returns>
        public static IWebHostBuilder UseDefaults(this IWebHostBuilder builder,bool enableFluentValidationAspnetPipeline)
        {
            builder.ConfigureServices((WebHostBuilderContext context, IServiceCollection services) =>
            {
                services.EnableResponseCompression();

                services.AddControllers(options =>
                {
                    options.RespectBrowserAcceptHeader = true; // false by default
                })
                .AddProtobufFormatters()
                .AddXmlDataContractSerializerFormatters()
                .ConfigureInvalidModelStateResponse();

                if (enableFluentValidationAspnetPipeline)
                {
                    services.AddFluentValidationAutoValidation();
                    services.AddFluentValidationClientsideAdapters();
                }

                services.AddValidatorsFromAssemblies(AssemblyExtensions.GetAllServiceAssemblies());

                //Fluent validation interceptor
                services.AddTransient<IValidatorInterceptor, ForbiddenErrorCodeInterceptor>();

                services.AddCodeFirstGrpc(config =>
                {
                    config.ResponseCompressionLevel = System.IO.Compression.CompressionLevel.Optimal;
                    config.ResponseCompressionAlgorithm = "gzip";
                    config.Interceptors.Add<RequestLoggerInterceptor>();
                    config.Interceptors.Add<RequestValidatorInterceptor>();
                });

                services.AddDataProtection(context.Configuration);

                services.AddAuthentication(context.Configuration);

                services.TryAddSingleton<ISecureTokenUserProvider, SecureTokenUserProvider>();

                services.TryAddSingleton(Log.Logger);

                services.AddApiLogWriter();

                services.AddHealthChecks();

                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Swagger API", Version = "v1" });
                    c.OperationFilter<AuthorizationInputOperationFilter>();
                });

                services.AddSwaggerGenNewtonsoftSupport();

                //Configure "Default" cors policy
                services.ConfigureDefaultCors(context.Configuration);

                //Cache-store dependencies
                services.AddCacheDependencies();

                //Add EndPointMiddlewareHandler<>
                services.TryAddSingleton(typeof(EndPointMiddlewareHandler<>));

                services.RegisterBaseClient(bypassSslCheck: context.HostingEnvironment.IsDevelopment());

                services.RegisterBaseProtoBufClient(bypassSslCheck: context.HostingEnvironment.IsDevelopment());

                services.RegisterExternalClient(bypassSslCheck: context.HostingEnvironment.IsDevelopment());

                //Add Sqlserver with health checks
                services.AddSqlServerWithHealthCheck(context.Configuration);

                //Adding SaveRequestContextFilter and its dependencies
                services.AddRequestContextAccessor();

                //Adding actionfilter for request logging as mvc pipeline would make binded arguments available
                services.AddFilter<RequestLoggingFilter>(-2100);

                if (!enableFluentValidationAspnetPipeline)
                {
                    services.AddFilter<FluentValidate>();
                }

                //Add HtpConext Accessor
                services.AddHttpContextAccessor();
            });

            return builder;
        }
    }
}