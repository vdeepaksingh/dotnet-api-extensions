using FluentValidation;

using Grpc.Core;
using Grpc.Core.Interceptors;

namespace dotnet_api_extensions.Grpc
{
    /// <summary>
    /// Interceptor to validate request models
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    public class RequestValidatorInterceptor : Interceptor
    {

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="continuation"></param>
        /// <returns></returns>
        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request,
                                          ServerCallContext context,
                                          UnaryServerMethod<TRequest, TResponse> continuation)
        {
            var httpContext = context.GetHttpContext();

            var validator = httpContext?.RequestServices.GetService<IValidator<TRequest>>();

            if (validator != null)
            {
                var validationResults = await validator.ValidateAsync(request);

                if (!validationResults.IsValid && validationResults.Errors.Count > 0)
                {
                    var errors = string.Join('\n', validationResults.Errors
                                    .Select(v => v.ErrorMessage).Distinct());

                    throw new RpcException(new Status(StatusCode.InvalidArgument, errors));
                }
            }

            return await continuation(request, context);
        }
    }
}
