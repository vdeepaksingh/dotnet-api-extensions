using dotnet_api_extensions.Authentication;
using dotnet_api_extensions.Caches;
using dotnet_api_extensions.Grpc;
using dotnet_api_extensions.Serilog;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;

using HealthChecks.UI.Client;
using Serilog;

namespace dotnet_api_extensions
{
    /// <summary>
    /// Adds components to application builder
    /// </summary>
    public static class AppBuilderExtensions
    {
        /// <summary>
        /// All the defaults at one place.
        /// Adds https redirection, routing, authorization, default CORS, end points mapping, health checks, swagger, swagger UI and cache store repo end point.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseDefaults(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            //Order of forwarded header options: https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-3.1#fhmo

            var forwardedHeadersOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };

            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                // Adding support for ForwardedHeaders to fetch IP address
                app.UseForwardedHeaders(forwardedHeadersOptions);
            }
            else
            {
                // Adding support for ForwardedHeaders to fetch IP address
                app.UseForwardedHeaders(forwardedHeadersOptions);

                app.UseHsts();
            }

            app.UseHttpsRedirection();

            //Static files under wwwroot should be returned without executing other middlewares. UseStaticFiles work as terminal middleware
            app.UseStaticFiles();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(
            options =>
            {
                string swaggerJsonBasePath = string.IsNullOrWhiteSpace(options.RoutePrefix) ? "." : "..";
                options.SwaggerEndpoint($"{swaggerJsonBasePath}/swagger/v1/swagger.json", "API V1");
            });

            app.UseRouting();

            app.UseGrpcWeb(new GrpcWebOptions() { DefaultEnabled = true });

            app.UseCors();

            app.UseAuthentication();

            //Adding support to read oauth bearer token set by legacy app
            app.UseMiddleware<OAuthBearerTokenMiddleware>();

            //Adding username to each log (even serilog request logging)
            app.UseMiddleware<LogUserNameMiddleware>();

            app.UseSerilogRequestLogging(opts =>
            {
                opts.MessageTemplate = Constants.RequestLogTemplate;
                opts.EnrichDiagnosticContext = HttpRequestLogEnricher.EnrichFromRequest;
            });

            app.UseMiddleware<SaveRequestContextMiddleware>();

            //Grpc authorization needs extra code
            app.UseWhen((ctx) => IsGrpcRequest(ctx), appBuilder => appBuilder.UseMiddleware<HandleGrpcAuthorizationMiddleware>());

            // Adding health checks
            app.UseHealthChecks(path: "/hc", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            //Add cacheStore repo endpoints
            app.AddCacheStoreRepoEndPoints();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapGrpcServices();
            });

            return app;
        }

        private static bool IsGrpcRequest(HttpContext ctx)
        {
            var grpcType = "application/grpc";

            if (string.IsNullOrEmpty(ctx.Request.ContentType))
            {
                //Validate accept header then
                return ctx.Request.Headers["Accept"].Any(type => type.StartsWith(grpcType));
            }

            return ctx.Request.ContentType.StartsWith(grpcType);
        }
    }
}