using Microsoft.AspNetCore.ResponseCompression;

namespace dotnet_api_extensions.Response
{
    internal static class CompressionExtensions
    {
        public static IServiceCollection EnableResponseCompression(this IServiceCollection services)
        {
            services.Configure<GzipCompressionProviderOptions>(options => options.Level = System.IO.Compression.CompressionLevel.Fastest);

            services.AddResponseCompression(options =>
            {
                // Gzip and Brotli compression providers are added by default. So, commenting out below line
                // https://docs.microsoft.com/en-us/aspnet/core/performance/response-compression?view=aspnetcore-3.1
                // options.Providers.Add<GzipCompressionProvider>();
                options.EnableForHttps = true;
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
                        {
                               "application/x-protobuf",
                               "application/protobuf",
                               "application/x-google-protobuf",
                               "application/grpc"
                        });
            });
            return services;
        }
    }
}
