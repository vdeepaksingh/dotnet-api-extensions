using dotnet_api_extensions.ServiceCollection;

namespace dotnet_api_extensions.ServiceCollection
{
    /// <summary>
    /// Extensions to add assemblies and types to automapper
    /// </summary>
    public static class AutomapperExtensions
    {
        /// <summary>
        /// This loads all the referenced assemblies. This can be slow, so use judiciously.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="reflectionOnly"></param>
        /// <param name="includedAssemblyPrefix"></param>
        /// <returns></returns>
        public static IServiceCollection AddAutoMapper(this IServiceCollection services, bool reflectionOnly = false, string includedAssemblyPrefix = "")
        {
            var assemblies = reflectionOnly ? AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies()
                                            : AppDomain.CurrentDomain.GetAssemblies();
            var includedAssemblies = string.IsNullOrEmpty(includedAssemblyPrefix)
                ? assemblies
                : assemblies.Where(x => x.FullName.StartsWith(includedAssemblyPrefix));

            services.AddAutoMapper(includedAssemblies);
            return services;
        }
    }
}