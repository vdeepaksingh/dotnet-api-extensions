using dotnet_api_extensions.HealthChecks;
using dotnet_api_extensions.Sql;

using Microsoft.Extensions.DependencyInjection.Extensions;

namespace dotnet_api_extensions.ServiceCollection
{
    /// <summary>
    /// Extensions around Musafir.Libaries.SqlServer and SqlServer healthchecks
    /// </summary>
    public static class SqlServerExtensions
    {
        /// <summary>
        /// Add SqlServer dependencies with necessary healthchecks around it
        /// </summary>
        /// <param name="services"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static IServiceCollection AddSqlServerWithHealthCheck(this IServiceCollection services, string connectionString)
        {
            services.TryAddSingleton(new SqlDbConnectionFactory(connectionString));
            services.TryAddScoped<IUnitOfWork, UnitOfWork>();
            services.AddSqlHealthCheck(connectionString);
            return services;
        }

        /// <summary>
        /// Add SqlServer dependencies with necessary healthchecks around it
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="connectionStringName">connecion string name to be read from configuration object</param>
        /// <returns></returns>
        public static IServiceCollection AddSqlServerWithHealthCheck(this IServiceCollection services, IConfiguration configuration, string connectionStringName = "SQLDatabase")
        {
            var connectionString = configuration.GetConnectionString(connectionStringName);
            services.AddSqlServerWithHealthCheck(connectionString);
            return services;
        }

        /// <summary>
        /// Add sql server healthchecks
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="connectionStringName">connecion string name to be read from configuration object</param>
        /// <returns></returns>
        public static IServiceCollection AddSqlHealthCheck(this IServiceCollection services, IConfiguration configuration, string connectionStringName = "SQLDatabase")
        {
            var connectionString = configuration.GetConnectionString(connectionStringName);
            services.AddSqlHealthCheck(connectionString);
            return services;
        }

        /// <summary>
        /// Add sql server healthchecks
        /// </summary>
        /// <param name="services"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static IServiceCollection AddSqlHealthCheck(this IServiceCollection services, string connectionString)
        {
            //Add this sql connection string to HealthChecks
            new HealthChecksBuilder(services).AddSqlServer(connectionString);
            return services;
        }
    }
}