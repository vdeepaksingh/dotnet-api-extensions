using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;

using Microsoft.AspNetCore.Mvc;

namespace dotnet_api_extensions.Response
{
    /// <summary>
    /// Fluent interceptor to append error code with error message
    /// </summary>
    public class ForbiddenErrorCodeInterceptor : IValidatorInterceptor
    {
        internal static string ForbiddenErrorCode = "403";
        internal static string ForbiddenErrorCodeMsg = $"Forbidden_{ForbiddenErrorCode}";

        /// <summary>
        /// After event
        /// </summary>
        /// <param name="actionContext"></param>
        /// <param name="commonContext"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public ValidationResult AfterAspNetValidation(ActionContext actionContext, IValidationContext commonContext, ValidationResult result)
        {
            var projection = result.Errors.Select(failure =>
            {
                var errorMsg = failure.ErrorMessage;
                if (failure.ErrorCode?.Equals(ForbiddenErrorCode) ?? false)
                {
                    errorMsg = ForbiddenErrorCodeMsg;
                }
                return new ValidationFailure(failure.PropertyName, errorMsg);
            });

            return new ValidationResult(projection);
        }

        /// <summary>
        /// Before event
        /// </summary>
        /// <param name="actionContext"></param>
        /// <param name="commonContext"></param>
        /// <returns></returns>
        public IValidationContext BeforeAspNetValidation(ActionContext actionContext, IValidationContext commonContext)
        {
            return commonContext;
        }
    }
}