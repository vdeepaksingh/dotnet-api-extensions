using Serilog;

namespace dotnet_api_extensions
{
    /// <summary>
    ///Host with default configuration
    /// </summary>
    public static class HostBuilder
    {
        /// <summary>
        /// Creates default builder using IHostBuilder.UseDefaults
        /// </summary>
        /// <typeparam name="TStartup"></typeparam>
        /// <param name="args"></param>
        /// <param name="findAndAddSharedConfig"></param>
        /// <returns></returns>
        public static IHostBuilder CreateDefaultBuilder<TStartup>(string[] args, bool findAndAddSharedConfig = true) where TStartup : class 
            => CreateDefaultBuilderInternal<TStartup>(args, findAndAddSharedConfig, false);

        /// <summary>
        /// Creates default builder using IHostBuilder.UseDefaults
        /// </summary>
        /// <typeparam name="TStartup"></typeparam>
        /// <param name="args"></param>
        /// <param name="hostConfiguration"></param>
        /// <returns></returns>
        public static IHostBuilder CreateDefaultBuilder<TStartup>(string[] args, HostConfiguration hostConfiguration) where TStartup : class 
            => CreateDefaultBuilderInternal<TStartup>(args, hostConfiguration.FindAndAddSharedConfig, hostConfiguration.EnableFluentValidationAspNetAutoPipeline);

        private static IHostBuilder CreateDefaultBuilderInternal<TStartup>(string[] args,
            bool findAndAddSharedConfig,
            bool enableFluentValidationAspnetPipeline) where TStartup : class
        {
            try
            {
                return Host.CreateDefaultBuilder(args)
                    .UseDefaults(findAndAddSharedConfig)
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseDefaults(enableFluentValidationAspnetPipeline);
                        webBuilder.UseStartup<TStartup>();
                    });
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host builder error");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }

    /// <summary>
    /// Configuration to setup custom host
    /// </summary>
    public class HostConfiguration
    {
        /// <summary>
        /// To enable addition of shared config
        /// </summary>
        public bool FindAndAddSharedConfig { get; set; } = true;

        /// <summary>
        /// To disable fluent validation automatic pipeline. FluentValidation 11 does not recommend automatic pipeline if async validation is used
        /// </summary>
        public bool EnableFluentValidationAspNetAutoPipeline { get; set; }
    }
}