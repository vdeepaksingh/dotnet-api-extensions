namespace dotnet_api_extensions
{
    /// <summary>
    /// Base startup class
    /// To support default configure
    /// </summary>
    public class BaseStartup
    {
        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940.
        /// Override this method to provide additional services.
        /// Default services are provided through WebHostBuilderExtensions
        /// </summary>
        /// <param name="services"></param>
        public virtual void ConfigureServices(IServiceCollection services)
        {

        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// Override this method if additional middleware code is needed.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env) => app.UseDefaults(env);
    }
}