namespace dotnet_api_extensions.Grpc
{
    /// <summary>
    /// Attribute to be used for identifying and registering GrpcServices
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public class GrpcService : Attribute
    {
    }
}