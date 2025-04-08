using Microsoft.AspNetCore.Mvc;

namespace dotnet_api_extensions.Response
{
    internal static class InvalidModelStateExtensions
    {
        public static IMvcBuilder ConfigureInvalidModelStateResponse(this IMvcBuilder mvcBuilder)
        {
            mvcBuilder.ConfigureApiBehaviorOptions(options =>
             {
                 options.InvalidModelStateResponseFactory = context =>
                 {
                     var errorsObj = context.ModelState.Values.Where(v => v.Errors.Count > 0)
                                    .SelectMany(v => v.Errors)
                                    .Select(v => v.ErrorMessage).Distinct().ToList();

                     if (errorsObj.Any(msg => msg.Equals(ForbiddenErrorCodeInterceptor.ForbiddenErrorCodeMsg)))
                     {
                         return new ForbidResult();
                     }

                     var errors = string.Join('\n', errorsObj);

                     return new OkObjectResult(new
                     {
                         IsSuccess = false,
                         ErrorMessage = errors
                     });
                 };
             });
            return mvcBuilder;
        }
    }
}