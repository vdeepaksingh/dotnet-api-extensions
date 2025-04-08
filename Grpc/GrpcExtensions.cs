using System.Reflection;

using dotnet_api_extensions.Extensions;

using AssemblyExtensions = dotnet_api_extensions.Extensions.AssemblyExtensions;

namespace dotnet_api_extensions.Grpc
{
    /// <summary>
    /// To add grpc services
    /// </summary>
    internal static class GrpcExtensions
    {
        public static IEndpointRouteBuilder MapGrpcServices(this IEndpointRouteBuilder endpointRouteBuilder, 
            string[] includedAssemblies = null,
            string[] excludedAssemblies = null)
        {
            var allAssemblies = AssemblyExtensions.GetAllServiceAssemblies(includedAssemblies, excludedAssemblies);

            foreach (var assembly in allAssemblies)
            {
                foreach (var type in assembly.GetTypesWithAttribute<GrpcService>())
                {
                    endpointRouteBuilder.MapGrpcService(type);
                }
            }

            return endpointRouteBuilder;
        }

        public static IEndpointRouteBuilder MapGrpcService(this IEndpointRouteBuilder endpointRouteBuilder, Type type)
        {
            //using reflection to perform service mapping of generic type
            var method = typeof(GrpcEndpointRouteBuilderExtensions).GetMethod(nameof(GrpcEndpointRouteBuilderExtensions.MapGrpcService));
            var generic = method.MakeGenericMethod(type);
            generic.Invoke(null, [endpointRouteBuilder]);

            return endpointRouteBuilder;
        }

        private static IEnumerable<Assembly> GetOnlyExecutingAssembly()
        {
            return new List<Assembly> { Assembly.GetExecutingAssembly() };
        }
    }
}