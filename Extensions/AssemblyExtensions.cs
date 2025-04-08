using System.Reflection;

namespace dotnet_api_extensions.Extensions
{
    /// <summary>
    /// Fetch information from passed assembly
    /// </summary>
    public static class AssemblyExtensions
    {
        /// <summary>
        /// Returns all the classes which have TAttribute defined
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetTypesWithAttribute<TAttribute>(this Assembly assembly) where TAttribute : Attribute
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (Attribute.IsDefined(type, typeof(TAttribute)))
                    yield return type;
            }
        }

        /// <summary>
        ///  Returns all service assemblies
        /// </summary>
        /// <param name="includedAssemblies"></param>
        /// <param name="excludedAssemblies"></param>
        /// <returns></returns>
        public static IEnumerable<Assembly> GetAllServiceAssemblies(string[] includedAssemblies = null, string[] excludedAssemblies = null)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            includedAssemblies ??= [];
            excludedAssemblies ??= [];

            if (includedAssemblies.Length > 0 || excludedAssemblies.Length > 0)
            {
                return assemblies.Where(x => includedAssemblies.Any(i => x.FullName.StartsWith(i))
                                                                && !excludedAssemblies.Any(e => x.FullName.StartsWith(e)));
            }
            return assemblies;
        }
    }
}