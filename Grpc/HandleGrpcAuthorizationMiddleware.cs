using dotnet_api_extensions.Authorization;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_api_extensions.Grpc
{
    /// <summary>
    /// AuthorizeUser attribute is not executed in Grpc flow as MVC pipeline does not kick in here.
    /// So, calling the same attribute through a middleware.
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    /// <param name="next"></param>
    /// <param name="logger"></param>
    internal class HandleGrpcAuthorizationMiddleware(RequestDelegate next,
        ILogger<HandleGrpcAuthorizationMiddleware> logger)
    {
        public async Task Invoke(HttpContext context)
        {
            var endPoint = context.GetEndpoint();

            if (endPoint == null)
            {
                logger.LogError("{summary}", "No end point for grpc found");
                throw new Exception("No end point for grpc");
            }

            var authorizeUserMetadata = endPoint.Metadata.GetMetadata<AuthorizeUser>();

            if (authorizeUserMetadata != null)
            {
                var authorizeResult = await authorizeUserMetadata.OnAuthorizationAsyncInternal(context);

                if (authorizeResult is ForbidResult)
                {
                    await context.ForbidAsync();
                    return;
                }

                if (authorizeResult is UnauthorizedResult)
                {
                    await context.ChallengeAsync();
                    return;
                }
            }

            await next(context);
        }
    }
}