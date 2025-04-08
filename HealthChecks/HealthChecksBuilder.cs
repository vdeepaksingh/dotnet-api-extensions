using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace dotnet_api_extensions.HealthChecks
{
    /// <summary>
    /// Implementation of Microsoft.Extensions.DependencyInjection.IHealthChecksBuilder to configure healthChecks dynamically
    /// </summary>
    /// <remarks>
    /// Default constructor
    /// </remarks>
    /// <param name="services"></param>
    public class HealthChecksBuilder(IServiceCollection services) : IHealthChecksBuilder
    {

        /// <summary>
        /// Service collection used for registrations
        /// </summary>
        public IServiceCollection Services { get; } = services;

        /// <summary>
        /// Add a new HealthCheckRegistration
        /// </summary>
        /// <param name="registration"></param>
        /// <returns></returns>
        public IHealthChecksBuilder Add(HealthCheckRegistration registration)
        {
            ArgumentNullException.ThrowIfNull(registration);

            Services.Configure<HealthCheckServiceOptions>(options =>
            {
                options.Registrations.Add(registration);
            });

            return this;
        }
    }
}