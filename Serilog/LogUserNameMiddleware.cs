using Serilog.Context;

namespace dotnet_api_extensions.Serilog
{
    internal class LogUserNameMiddleware(RequestDelegate next)
    {
        public async Task Invoke(HttpContext context)
        {
            using (LogContext.PushProperty("UserName", context.User.Identity.Name))
            {
                await next(context);
            }
        }
    }
}