namespace dotnet_api_extensions.Serilog
{
    internal static class Constants
    {
        public const string RequestLogTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms {DetailsFilePath}";
    }
}